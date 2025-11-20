using GameServices.Data;
using Microsoft.EntityFrameworkCore;
using SharedModels.Entities;
using GameServices.Logic; 
using SharedModels; 

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS pour le client sur 5000
const string CorsPolicy = "AllowClient";
builder.Services.AddCors(o => o.AddPolicy(CorsPolicy, p =>
    p.WithOrigins("http://localhost:5000").AllowAnyHeader().AllowAnyMethod()
));

// EF Core InMemory
builder.Services.AddDbContext<GameDbContext>(opt =>
    opt.UseInMemoryDatabase("blazor-game-db"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors(CorsPolicy);

// Compatibilité avec le client actuel (Test Dungeon)
app.MapGet("/api/dungeon/new", (int? min, int? max) =>
{
    var rooms = DungeonGenerator.GenerateDungeon(min ?? 3, max ?? 5); 
    return Results.Ok(rooms);
});

// CRUD minimal Players
app.MapPost("/api/players", async (GameDbContext db, PlayerCreate dto) =>
{
    var p = new Player { UserName = dto.UserName.Trim() };
    db.Players.Add(p);
    await db.SaveChangesAsync();
    return Results.Created($"/api/players/{p.Id}", p);
});

app.MapGet("/api/players", async (GameDbContext db) =>
    Results.Ok(await db.Players.OrderBy(p => p.Id).ToListAsync()));

app.MapGet("/api/players/{id:int}", async (GameDbContext db, int id) =>
{
    var p = await db.Players.Include(x => x.Adventures).FirstOrDefaultAsync(x => x.Id == id);
    return p is null ? Results.NotFound() : Results.Ok(p);
});

// -----------------------------------------------------------
// Démarrer une aventure
// -----------------------------------------------------------
app.MapPost("/api/adventures/start", async (GameDbContext db, StartAdventure dto) =>
{
    var player = await db.Players.FindAsync(dto.PlayerId);
    if (player is null) return Results.BadRequest("Player not found");

    var adv = new Adventure { PlayerId = dto.PlayerId, CreatedAt = DateTime.UtcNow };
    db.Adventures.Add(adv);
    await db.SaveChangesAsync();

    // Génération du donjon
    var rooms = DungeonGenerator.GenerateDungeon(dto.MinRooms ?? 3, dto.MaxRooms ?? 5); 
    
    // CORRECTION MAJEURE: On renvoie l'objet 'adv' complet, pas juste l'ID
    return Results.Created($"/api/adventures/{adv.Id}", new StartPayload(adv, rooms));
});

// -----------------------------------------------------------
// Terminer une aventure (Sauvegarde score + historique)
// -----------------------------------------------------------
app.MapPost("/api/adventures/{id:int}/finish", async (GameDbContext db, int id, FinishAdventure dto) =>
{
    var adv = await db.Adventures.FirstOrDefaultAsync(a => a.Id == id);
    if (adv is null) return Results.NotFound();

    adv.Score = dto.Score;
    adv.FinishedAt = DateTime.UtcNow;

    // CORRECTION: Mapping des DTOs (int) vers les Entités (Enum)
    if (dto.Rooms != null)
    {
        adv.Rooms = dto.Rooms.Select(r => new RoomPlay
        {
            Index = r.Index,
            Type = (RoomType)r.Type,           // Cast int -> Enum
            Difficulty = r.Difficulty,
            Action = (PlayerAction)r.Action,   // Cast int -> Enum
            Points = r.Points
        }).ToList();
    }
    else 
    {
        adv.Rooms = new List<RoomPlay>();
    }

    await db.SaveChangesAsync();
    return Results.Ok(adv);
});

// Leaderboard
app.MapGet("/api/leaderboard", async (GameDbContext db, int top = 10) =>
{
    var data = await db.Adventures
        .Where(a => a.FinishedAt != null)
        .OrderByDescending(a => a.Score)
        .Take(top)
        .Select(a => new { a.Id, a.Score, a.PlayerId, a.FinishedAt })
        .ToListAsync();

    return Results.Ok(data);
});

app.Run();

// -----------------------------------------------------------
// DTOs (Data Transfer Objects)
// -----------------------------------------------------------
record PlayerCreate(string UserName);
record StartAdventure(int PlayerId, int? MinRooms, int? MaxRooms);

// CORRECTION: Le StartPayload renvoie maintenant l'objet Adventure complet
public record StartPayload(Adventure Adventure, IReadOnlyList<Room> Rooms);

// CORRECTION: FinishAdventure utilise RoomPlayDto pour recevoir les données brutes (int)
public record FinishAdventure(int Score, List<RoomPlayDto>? Rooms);
public record RoomPlayDto(int Index, int Type, int Difficulty, int Action, int Points);