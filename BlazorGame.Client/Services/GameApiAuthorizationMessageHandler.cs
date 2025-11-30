using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace BlazorGame.Client.Services
{
    public class GameApiAuthorizationMessageHandler : AuthorizationMessageHandler
    {
        public GameApiAuthorizationMessageHandler(IAccessTokenProvider provider,
            NavigationManager navigation)
            : base(provider, navigation)
        {
            ConfigureHandler(
                authorizedUrls: new[] { "http://localhost:5001" }, // On autorise explicitement le port 5001
                scopes: new[] { "roles" }); // Les scopes requis
        }
    }
}