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
            // 1. On récupère l'utilisateur de base (Authentifié par défaut)
            var initialUser = await base.CreateUserAsync(account, options);

            if (initialUser?.Identity == null || !initialUser.Identity.IsAuthenticated)
            {
                return initialUser ?? new ClaimsPrincipal(new ClaimsIdentity());
            }

            var identity = (ClaimsIdentity)initialUser.Identity;

            try
            {
                // 2. On essaie d'extraire les rôles sans faire tout exploser
                if (account?.AdditionalProperties != null &&
                    account.AdditionalProperties.TryGetValue("realm_access", out var realmAccess) && 
                    realmAccess is JsonElement realmElement)
                {
                    // CAS A : Objet { "roles": [...] }
                    if (realmElement.ValueKind == JsonValueKind.Object && 
                        realmElement.TryGetProperty("roles", out var rolesElement) &&
                        rolesElement.ValueKind == JsonValueKind.Array)
                    {
                        AddRolesToIdentity(identity, rolesElement);
                    }
                    // CAS B : Tableau Direct [...]
                    else if (realmElement.ValueKind == JsonValueKind.Array)
                    {
                        AddRolesToIdentity(identity, realmElement);
                    }
                }
            }
            catch (Exception ex)
            {
                // 🚨 EN CAS D'ERREUR : On ne plante pas l'appli !
                // On logue juste l'erreur dans la console F12 pour comprendre.
                Console.WriteLine($"[CRITICAL ERROR] Erreur lecture rôles : {ex.Message}");
            }

            return initialUser;
        }

        private void AddRolesToIdentity(ClaimsIdentity identity, JsonElement rolesArray)
        {
            foreach (var role in rolesArray.EnumerateArray())
            {
                string? roleValue = null;

                try
                {
                    if (role.ValueKind == JsonValueKind.String)
                    {
                        roleValue = role.GetString();
                    }
                    else if (role.ValueKind == JsonValueKind.Object)
                    {
                        // DÉTECTION "POUPÉE RUSSE" (Ton bug probable)
                        // Si l'objet contient lui-même une liste "roles" : { "roles": ["player"] }
                        if (role.TryGetProperty("roles", out var innerRoles) && innerRoles.ValueKind == JsonValueKind.Array)
                        {
                             foreach (var inner in innerRoles.EnumerateArray())
                             {
                                 if (inner.ValueKind == JsonValueKind.String)
                                 {
                                     // On ajoute directement ici pour éviter la complexité
                                     var val = inner.GetString();
                                     if (!string.IsNullOrEmpty(val)) AddClaimSafely(identity, val);
                                 }
                             }
                             // On a traité cet objet, on passe au suivant
                             continue; 
                        }
                        
                        // Sinon c'est un objet rôle standard { "name": "player" } ou { "role": "player" }
                        if (role.TryGetProperty("name", out var nameProp) && nameProp.ValueKind == JsonValueKind.String)
                        {
                            roleValue = nameProp.GetString();
                        }
                        else if (role.TryGetProperty("role", out var roleProp) && roleProp.ValueKind == JsonValueKind.String)
                        {
                            roleValue = roleProp.GetString();
                        }
                    }
                }
                catch
                {
                    // Ignorer un rôle malformé individuel
                }

                if (!string.IsNullOrEmpty(roleValue))
                {
                    AddClaimSafely(identity, roleValue);
                }
            }
        }

        private void AddClaimSafely(ClaimsIdentity identity, string value)
        {
            // On ajoute les DEUX types de clés pour être sûr que [Authorize] soit content
            if (!identity.HasClaim(c => c.Type == "role" && c.Value == value))
            {
                identity.AddClaim(new Claim("role", value));
            }
            
            if (!identity.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == value))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, value));
            }
        }
    }
}