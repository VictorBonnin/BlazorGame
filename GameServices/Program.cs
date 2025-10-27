using GameServices.Data;
using Microsoft.EntityFrameworkCore;
using SharedModels.Entities;   // <-- pas SharedModels;

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

// util pour générer les salles
static List<Room> GenerateRooms(int min = 3, int max = 5)
{
    var rng = Random.Shared;
    min = Math.Max(1, min);
    max = Math.Max(min, max);

    var count = rng.Next(min, max + 1);
    var rooms = new List<Room>(count);
    for (int i = 1; i <= count; i++)
    {
        var type = (RoomType)rng.Next(0, 3);
        var diff = rng.Next(1, 6);
        rooms.Add(new Room(i, type, diff));
    }
    return rooms;
}

// Compatibilité avec le client actuel (NewAdventure)
app.MapGet("/api/dungeon/new", (int? min, int? max) =>
{
    var rooms = GenerateRooms(min ?? 3, max ?? 5);
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

// Démarrer une aventure -> renvoie ID + salles
app.MapPost("/api/adventures/start", async (GameDbContext db, StartAdventure dto) =>
{
    var player = await db.Players.FindAsync(dto.PlayerId);
    if (player is null) return Results.BadRequest("Player not found");

    var adv = new Adventure { PlayerId = dto.PlayerId, CreatedAt = DateTime.UtcNow };
    db.Adventures.Add(adv);
    await db.SaveChangesAsync();

    var rooms = GenerateRooms(dto.MinRooms ?? 3, dto.MaxRooms ?? 5);
    return Results.Created($"/api/adventures/{adv.Id}", new StartPayload(adv.Id, rooms));
});

// Terminer une aventure
app.MapPost("/api/adventures/{id:int}/finish", async (GameDbContext db, int id, FinishAdventure dto) =>
{
    var adv = await db.Adventures.FirstOrDefaultAsync(a => a.Id == id);
    if (adv is null) return Results.NotFound();

    adv.Score = dto.Score;
    adv.FinishedAt = DateTime.UtcNow;
    adv.Rooms = dto.Rooms ?? new();
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

// DTOs
record PlayerCreate(string UserName);
record StartAdventure(int PlayerId, int? MinRooms, int? MaxRooms);
record FinishAdventure(int Score, List<RoomPlay>? Rooms);
record StartPayload(int AdventureId, List<Room> Rooms);
