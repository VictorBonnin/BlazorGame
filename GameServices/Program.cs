using GameServices.Data;
using GameServices.Logic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims; // 👈 NÉCESSAIRE
using System.Text.Json;       // 👈 NÉCESSAIRE

var builder = WebApplication.CreateBuilder(args);

// --- CONFIGURATION ---

// 2. Configuration de l'Authentification (Keycloak)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "http://localhost:8080/realms/blazorgame-realm";
        options.RequireHttpsMetadata = false; 
        
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            NameClaimType = "preferred_username",
            // 👇 Important : on dit à .NET que les rôles s'appellent "role" en interne
            RoleClaimType = ClaimTypes.Role 
        };

        // 👇 C'EST ICI LA MAGIE : On intercepte le token validé pour extraire les rôles Keycloak
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var principal = context.Principal;
                if (principal?.Identity is ClaimsIdentity identity)
                {
                    // Keycloak met les rôles dans "realm_access": { "roles": ["admin", ...] }
                    var realmAccess = identity.FindFirst("realm_access")?.Value;
                    if (!string.IsNullOrEmpty(realmAccess))
                    {
                        try 
                        {
                            using var doc = JsonDocument.Parse(realmAccess);
                            if (doc.RootElement.TryGetProperty("roles", out var roles))
                            {
                                foreach (var role in roles.EnumerateArray())
                                {
                                    // Correction ici : on sécurise la récupération de la valeur
                                    var roleValue = role.GetString();
                                    
                                    if (!string.IsNullOrEmpty(roleValue))
                                    {
                                        // On ajoute le rôle seulement s'il est valide
                                        identity.AddClaim(new Claim(ClaimTypes.Role, roleValue));
                                    }
                                }
                            }
                        }
                        catch 
                        {
                            // En cas de JSON malformé, on ignore
                        }
                    }
                }
                return Task.CompletedTask;
            }
        };
    });

// 3. Configuration de l'Autorisation
builder.Services.AddAuthorization(options =>
{
    // Politique pour les admins
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
    
    // Politique pour les joueurs
    options.AddPolicy("Player", policy => policy.RequireAuthenticatedUser());
});

// 4. Services Contrôleurs
builder.Services.AddControllers()
    .AddJsonOptions(options => 
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<DungeonGenerator>();

// 5. CORS
builder.Services.AddCors(options => 
{
    options.AddDefaultPolicy(policy => 
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// 6. DB
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); 

app.Run();