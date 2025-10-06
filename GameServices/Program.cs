var builder = WebApplication.CreateBuilder(args);

// OpenAPI + Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger en dev (optionnel mais pratique)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Endpoints de test
app.MapGet("/", () => Results.Ok("GameServices API running"));
app.MapGet("/api/rooms/first", () => new { index = 1, type = "Combat", difficulty = 2 });

app.Run();
