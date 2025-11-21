using GameServices.Data;
using Microsoft.EntityFrameworkCore;
using SharedModels.Entities;
using GameServices.Logic; 
using SharedModels; 

var builder = WebApplication.CreateBuilder(args);

// 1. CONFIGURATION JSON (Indispensable pour éviter les boucles infinies)
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. CORS "BAZOOKA" (On autorise tout pour débloquer le développement)
// C'est ici que ça coinçait !
builder.Services.AddCors(options => 
{
    options.AddDefaultPolicy(policy => 
        policy.AllowAnyOrigin()  // Accepte http://localhost:5000, 127.0.0.1, etc.
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// EF Core InMemory
builder.Services.AddDbContext<GameDbContext>(opt =>
    opt.UseInMemoryDatabase("blazor-game-db"));

var app = builder.Build();

// 3. CONFIRMATION VISUELLE
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("*************************************************");
Console.WriteLine("* GameServices (Port 5001) : CORS OUVERT !      *");
Console.WriteLine("*************************************************");
Console.ResetColor();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Pas de redirection HTTPS (cause de conflits en local)
// app.UseHttpsRedirection();

// Active le CORS par défaut (avant les routes)
app.UseCors();

// -----------------------------------------------------------
// ROUTES API
// -----------------------------------------------------------

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

// Démarrer une aventure
app.MapPost("/api/adventures/start", async (GameDbContext db, StartAdventure dto) =>
{
    var player = await db.Players.FindAsync(dto.PlayerId);
    if (player is null) return Results.BadRequest("Player not found");

    var adv = new Adventure { PlayerId = dto.PlayerId, CreatedAt = DateTime.UtcNow };
    db.Adventures.Add(adv);
    await db.SaveChangesAsync();

    var rooms = DungeonGenerator.GenerateDungeon(dto.MinRooms ?? 3, dto.MaxRooms ?? 5); 
    return Results.Created($"/api/adventures/{adv.Id}", new StartPayload(adv, rooms));
});

// Terminer une aventure
app.MapPost("/api/adventures/{id:int}/finish", async (GameDbContext db, int id, FinishAdventure dto) =>
{
    var adv = await db.Adventures.FirstOrDefaultAsync(a => a.Id == id);
    if (adv is null) return Results.NotFound();

    adv.Score = dto.Score;
    adv.FinishedAt = DateTime.UtcNow;

    if (dto.Rooms != null)
    {
        adv.Rooms = dto.Rooms.Select(r => new RoomPlay
        {
            Index = r.Index,
            Type = (RoomType)r.Type,           
            Difficulty = r.Difficulty,
            Action = (PlayerAction)r.Action,   
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
        .Include(a => a.Player)
        .Where(a => a.FinishedAt != null)
        .OrderByDescending(a => a.Score)
        .Take(top)
        .ToListAsync();

    return Results.Ok(data);
});

app.Run();

// DTOs
record PlayerCreate(string UserName);
record StartAdventure(int PlayerId, int? MinRooms, int? MaxRooms);
public record StartPayload(Adventure Adventure, IReadOnlyList<Room> Rooms);
public record FinishAdventure(int Score, List<RoomPlayDto>? Rooms);
public record RoomPlayDto(int Index, int Type, int Difficulty, int Action, int Points);