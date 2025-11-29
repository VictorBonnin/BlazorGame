using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorGame.Client;
using BlazorGame.Client.Services;
using BlazorGame.Client.Logic;
using Blazored.LocalStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorage();

// 1. L'API de JEU est sur le port 5001
builder.Services.AddHttpClient("GameApi", client => 
    client.BaseAddress = new Uri("http://localhost:5001"));

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("GameApi"));

// 2. L'API d'AUTHENTIFICATION est sur le port 5200
builder.Services.AddHttpClient("AuthApi", client => 
    client.BaseAddress = new Uri("http://localhost:5200"));

// ---------------------------------------------

// Enregistrement des services

builder.Services.AddScoped<PlayerSessionService>(); 
builder.Services.AddSingleton<HintService>();
builder.Services.AddScoped<RoomHandlerFactory>(); 

builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Keycloak", options.ProviderOptions);
    options.ProviderOptions.ResponseType = "code";
    // Ces scopes permettent de récupérer les infos de l'utilisateur
    options.ProviderOptions.DefaultScopes.Add("openid");
    options.ProviderOptions.DefaultScopes.Add("profile");
    options.ProviderOptions.DefaultScopes.Add("email");
    options.ProviderOptions.DefaultScopes.Add("roles");
});

await builder.Build().RunAsync();