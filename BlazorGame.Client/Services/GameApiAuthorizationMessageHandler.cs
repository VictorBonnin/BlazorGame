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
                authorizedUrls: new[] { "http://localhost:5010" },
                scopes: new[] { "roles" });
        }
    }
}