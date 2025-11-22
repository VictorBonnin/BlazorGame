using GameServices.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- CONFIGURATION ---

// 1. Ajouter les services pour les Contrôleurs
builder.Services.AddControllers()
    .AddJsonOptions(options => 
    {
        // Gestion des références circulaires (comme avant)
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. CORS (Identique à ta version)
builder.Services.AddCors(options => 
{
    options.AddDefaultPolicy(policy => 
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// 3. Base de données (Identique)
builder.Services.AddDbContext<GameDbContext>(opt =>
    opt.UseInMemoryDatabase("blazor-game-db"));

var app = builder.Build();

// --- PIPELINE ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

// 4. C'est ici que ça change : on mappe les contrôleurs au lieu des lambdas
app.MapControllers(); 

app.Run();