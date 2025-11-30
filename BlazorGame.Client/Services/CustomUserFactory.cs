using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;

namespace BlazorGame.Client.Services
{
    public class CustomUserFactory : AccountClaimsPrincipalFactory<RemoteUserAccount>
    {
        public CustomUserFactory(IAccessTokenProviderAccessor accessor)
            : base(accessor)
        {
        }

        public override async ValueTask<ClaimsPrincipal> CreateUserAsync(
            RemoteUserAccount account,
            RemoteAuthenticationUserOptions options)
        {
            var initialUser = await base.CreateUserAsync(account, options);

            if (initialUser?.Identity == null || !initialUser.Identity.IsAuthenticated)
            {
                return initialUser ?? new ClaimsPrincipal(new ClaimsIdentity());
            }

            var identity = (ClaimsIdentity)initialUser.Identity;

            if (account?.AdditionalProperties != null &&
                account.AdditionalProperties.TryGetValue("realm_access", out var realmAccess) && 
                realmAccess is JsonElement realmElement)
            {
                // CAS A : Format Standard Keycloak -> { "roles": [...] }
                if (realmElement.ValueKind == JsonValueKind.Object && 
                    realmElement.TryGetProperty("roles", out var rolesElement) &&
                    rolesElement.ValueKind == JsonValueKind.Array)
                {
                    AddRolesToIdentity(identity, rolesElement);
                }
                // CAS B : Format "Aplati" -> [...]
                else if (realmElement.ValueKind == JsonValueKind.Array)
                {
                    AddRolesToIdentity(identity, realmElement);
                }
            }

            return initialUser;
        }

        private void AddRolesToIdentity(ClaimsIdentity identity, JsonElement rolesArray)
        {
            foreach (var role in rolesArray.EnumerateArray())
            {
                string? roleValue = null;

                if (role.ValueKind == JsonValueKind.String)
                {
                    roleValue = role.GetString();
                }
                else if (role.ValueKind == JsonValueKind.Object)
                {
                    // Gestion sécurisée des objets { "name": "player" }
                    if (role.TryGetProperty("name", out var nameProp) && nameProp.ValueKind == JsonValueKind.String)
                    {
                        roleValue = nameProp.GetString();
                    }
                    else if (role.TryGetProperty("role", out var roleProp) && roleProp.ValueKind == JsonValueKind.String)
                    {
                        roleValue = roleProp.GetString();
                    }
                    else
                    {
                        roleValue = role.ToString();
                    }
                }

                if (!string.IsNullOrEmpty(roleValue))
                {
                    // ✨ MAGIE ICI : On utilise identity.RoleClaimType
                    // Cela va automatiquement utiliser le standard "http://schemas.microsoft.com/..."
                    // que [Authorize] adore par défaut.
                    identity.AddClaim(new Claim(identity.RoleClaimType, roleValue));
                }
            }
        }
    }
}