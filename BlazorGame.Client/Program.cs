using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorGame.Client;
using BlazorGame.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// --- CONFIGURATION DES ADRESSES (ANNUAIRE) ---

// 1. L'API de JEU est sur le port 5001
builder.Services.AddHttpClient("GameApi", client => 
    client.BaseAddress = new Uri("http://localhost:5001"));

// On dit que par défaut, HttpClient utilise "GameApi" (pour Leaderboard, etc.)
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("GameApi"));

// 2. L'API d'AUTHENTIFICATION est sur le port 5200
builder.Services.AddHttpClient("AuthApi", client => 
    client.BaseAddress = new Uri("http://localhost:5200"));

// ---------------------------------------------

// Enregistrement des services
builder.Services.AddSingleton<PlayerSessionService>(); 
builder.Services.AddSingleton<HintService>();

await builder.Build().RunAsync();