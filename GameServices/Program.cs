using GameServices.Data;
using GameServices.Logic; // Nécessaire si tu utilises DungeonGenerator ici, sinon à retirer
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// --- CONFIGURATION ---

// 2. Configuration de l'Authentification (Keycloak)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // L'adresse de ton instance Keycloak (le Realm)
        options.Authority = "http://localhost:8080/realms/blazorgame-realm";
        
        // En développement (Docker/Http), on désactive la vérification HTTPS stricte
        options.RequireHttpsMetadata = false; 
        
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            // On valide que le token vient bien de notre Keycloak
            ValidateIssuer = true,
            // On valide l'audience (le client ID) - parfois "false" aide au debug si Keycloak n'envoie pas l'audience standard
            ValidateAudience = false, // Mettre à 'true' et définir ValidAudience = "blazorgame-client" pour plus de sécu
            // Permet de mapper le nom de l'utilisateur sur le champ 'preferred_username' du token
            NameClaimType = "preferred_username" 
        };
    });

// 3. Configuration de l'Autorisation (Rôles & Policies)
builder.Services.AddAuthorization(options =>
{
    // Politique pour les admins
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
    
    // Politique pour les joueurs (il suffit d'être authentifié pour être considéré comme joueur ici, ou avoir le rôle 'player')
    options.AddPolicy("Player", policy => policy.RequireAuthenticatedUser());
});

// 4. Ajouter les services pour les Contrôleurs
builder.Services.AddControllers()
    .AddJsonOptions(options => 
    {
        // Gestion des références circulaires
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Ajout de tes services métiers (DungeonGenerator semblait manquant dans ton snippet mais utile pour le jeu)
builder.Services.AddScoped<DungeonGenerator>();

// 5. CORS
builder.Services.AddCors(options => 
{
    options.AddDefaultPolicy(policy => 
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// 6. Base de données
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

// 7. Activation de la sécurité (Dans cet ordre précis !)
app.UseAuthentication(); // Vérifie qui est l'utilisateur (décode le Token)
app.UseAuthorization();  // Vérifie ce qu'il a le droit de faire (Rôles/Policies)

app.MapControllers(); 

app.Run();