using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;

namespace BlazorGame.Client.Services
{
    // On garde l'héritage standard
    public class CustomUserFactory : AccountClaimsPrincipalFactory<RemoteUserAccount>
    {
        public CustomUserFactory(IAccessTokenProviderAccessor accessor)
            : base(accessor)
        {
        }

        // CORRECTION : On utilise la signature exacte attendue par le runtime
        // Note: Pas de changement de signature majeur ici, mais on s'assure que tout est propre.
        public override async ValueTask<ClaimsPrincipal> CreateUserAsync(
            RemoteUserAccount account,
            RemoteAuthenticationUserOptions options)
        {
            // 1. Appel de la base (Important : c'est ici que l'erreur VTable peut pop si la base change)
            var initialUser = await base.CreateUserAsync(account, options);

            // 2. Sécurités
            if (initialUser?.Identity == null || !initialUser.Identity.IsAuthenticated)
            {
                return initialUser ?? new ClaimsPrincipal(new ClaimsIdentity());
            }

            var identity = (ClaimsIdentity)initialUser.Identity;

            // 3. Extraction des rôles
            if (account != null &&
                account.AdditionalProperties != null &&
                account.AdditionalProperties.TryGetValue("realm_access", out var realmAccess) && 
                realmAccess is JsonElement realmElement && 
                realmElement.TryGetProperty("roles", out var rolesElement))
            {
                if (rolesElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var role in rolesElement.EnumerateArray())
                    {
                        var roleValue = role.GetString();
                        if (!string.IsNullOrEmpty(roleValue))
                        {
                            // On utilise le RoleClaimType de l'identité existante
                            // C'est souvent "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                            // ou juste "role" selon la config OIDC.
                            identity.AddClaim(new Claim(identity.RoleClaimType, roleValue));
                        }
                    }
                }
            }

            return initialUser;
        }
    }
}