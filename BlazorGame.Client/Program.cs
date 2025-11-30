using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Blazored.LocalStorage;
using BlazorGame.Client;
using BlazorGame.Client.Services;
using BlazorGame.Client.Logic;
using BlazorGame.Client.Logic.Rooms;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// --- 1. CONFIGURATION API ---
builder.Services.AddScoped<GameApiAuthorizationMessageHandler>();
builder.Services.AddHttpClient("GameApi", client => 
    client.BaseAddress = new Uri("http://localhost:5001/")) 
    .AddHttpMessageHandler<GameApiAuthorizationMessageHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("GameApi"));

// --- 2. CONFIGURATION KEYCLOAK (OIDC) ---
builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Local", options.ProviderOptions);
    options.ProviderOptions.DefaultScopes.Add("roles");
})
.AddAccountClaimsPrincipalFactory<CustomUserFactory>(); // 👈 C'EST CETTE LIGNE QUI FAIT TOUTE LA DIFFÉRENCE

// --- 3. SERVICES ---
builder.Services.AddBlazoredLocalStorage(); 
builder.Services.AddScoped<HintService>();
builder.Services.AddScoped<PlayerSessionService>();

// --- 4. LOGIQUE JEU ---
builder.Services.AddScoped<RoomHandlerFactory>();
builder.Services.AddScoped<IRoomHandler, CombatRoomHandler>();
builder.Services.AddScoped<IRoomHandler, LootRoomHandler>();
builder.Services.AddScoped<IRoomHandler, MysteryRoomHandler>();
builder.Services.AddScoped<IRoomHandler, SanctuaryRoomHandler>();
builder.Services.AddScoped<IRoomHandler, ShopRoomHandler>();
builder.Services.AddScoped<IRoomHandler, TrapRoomHandler>();

await builder.Build().RunAsync();