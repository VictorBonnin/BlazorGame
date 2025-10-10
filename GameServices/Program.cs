using SharedModels;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS : autoriser le client Blazor (5000)
const string CorsPolicy = "AllowClient";
builder.Services.AddCors(o => o.AddPolicy(CorsPolicy, p =>
    p.WithOrigins("http://localhost:5000")
     .AllowAnyHeader()
     .AllowAnyMethod()
));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(CorsPolicy);

// Endpoints
app.MapGet("/", () => Results.Ok("GameServices API running"));

app.MapGet("/api/rooms/first", () => new { index = 1, type = "Combat", difficulty = 2 });

app.MapGet("/api/dungeon/new", (int? min, int? max) =>
{
    int minRooms = Math.Max(1, min ?? 3);
    int maxRooms = Math.Max(minRooms, max ?? 5);

    var rng = Random.Shared;
    int count = rng.Next(minRooms, maxRooms + 1);

    var rooms = new List<Room>(count);
    for (int i = 1; i <= count; i++)
    {
        var type = (RoomType)rng.Next(0, 3); // 0..2
        var difficulty = rng.Next(1, 6);     // 1..5
        rooms.Add(new Room(i, type, difficulty));
    }
    return Results.Ok(rooms);
});

app.Run();
