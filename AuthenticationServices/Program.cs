using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// 1. AJOUT DES SERVICES
builder.Services.AddOpenApi();

// --- MODIFICATION RADICALE : On utilise la politique PAR DÉFAUT ---
// Plus de nom de politique compliqué, on ouvre tout.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()  // Accepte tout le monde (localhost:5000, 127.0.0.1, etc.)
              .AllowAnyMethod()  // GET, POST, PUT...
              .AllowAnyHeader(); // Content-Type, etc.
    });
});
// ------------------------------------------------------------------

var app = builder.Build();

// 2. PREUVE DE VIE (Regarde ta console au lancement !)
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("*********************************************************");
Console.WriteLine("* NOUVELLE CONFIGURATION CORS CHARGÉE AVEC SUCCÈS !    *");
Console.WriteLine("*********************************************************");
Console.ResetColor();

// 3. CONFIGURATION DU PIPELINE
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Pas de redirection HTTPS pour éviter les conflits en local
// app.UseHttpsRedirection(); 

// --- ACTIVATION DU CORS (Doit être AVANT les routes) ---
app.UseCors(); // On appelle la politique par défaut définie plus haut
// -------------------------------------------------------

// 4. LES ROUTES
app.MapGet("/api/auth/session", () =>
{
    Console.WriteLine("--> Appel reçu sur /api/auth/session !"); // Log pour voir si l'appel arrive
    return Results.Ok<object?>(null);
});

app.MapPost("/api/auth/login", ([FromBody] object loginData) =>
{
    return Results.Ok(new { Message = "Login simulation" });
});

app.Run();