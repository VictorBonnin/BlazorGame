using GameServices.Data;
using GameServices.Logic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// --- CONFIGURATION ---

// 2. Configuration de l'Authentification (Keycloak)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // 🚨 MODIFICATION DOCKER 🚨
        // L'API utilise le réseau interne Docker pour parler à Keycloak ("keycloak" est le nom du service dans docker-compose)
        options.MetadataAddress = "http://keycloak:8080/realms/blazorgame-realm/.well-known/openid-configuration";
        
        // On désactive HTTPS pour le dev local
        options.RequireHttpsMetadata = false; 
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            // 🚨 CRUCIAL : On dit à l'API d'accepter l'émetteur "localhost" (celui vu par le navigateur)
            ValidIssuer = "http://localhost:8080/realms/blazorgame-realm",
            
            ValidateAudience = false, 
            NameClaimType = "preferred_username",
            RoleClaimType = ClaimTypes.Role
        };

        // 👇 REMISE EN PLACE DE LA LOGIQUE DES RÔLES (Indispensable pour l'Admin)
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
                                    var roleValue = role.GetString();
                                    if (!string.IsNullOrEmpty(roleValue))
                                    {
                                        identity.AddClaim(new Claim(ClaimTypes.Role, roleValue));
                                    }
                                }
                            }
                        }
                        catch 
                        {
                            // Ignorer JSON malformé
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
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
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