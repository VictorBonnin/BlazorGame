using System.Net.Http.Json;
using System.Net.Http;
using SharedModels.Entities;
using Blazored.LocalStorage;

namespace BlazorGame.Client.Services;

public class PlayerSessionService
{
    private readonly HttpClient _authHttp; // Client dédié à l'authentification (Port 5200)
    private readonly IHttpClientFactory _factory; // Pour générer le client Jeu (Port 5001)
    private readonly ILocalStorageService _localStorage;

    // On injecte la Factory et le LocalStorage
    public PlayerSessionService(IHttpClientFactory factory, ILocalStorageService localStorage)
    {
        _factory = factory;
        _authHttp = factory.CreateClient("AuthApi"); // Par défaut, on garde le lien vers Auth
        _localStorage = localStorage;
    }

    public int? CurrentPlayerId { get; private set; }
    public string? CurrentPlayerName { get; private set; }
    public bool IsAuthenticated => CurrentPlayerId.HasValue;
    
    public event Action? OnChange;

    // [Modifié] Méthode robuste anti-mode "Zombie"
    public async Task Initialize()
    {
        // 1. On lit la mémoire du navigateur
        var savedId = await _localStorage.GetItemAsync<int?>("playerId");
        var savedName = await _localStorage.GetItemAsync<string?>("playerName");

        if (savedId.HasValue)
        {
            try 
            {
                // 2. VÉRIFICATION CRUCIALE : On demande au serveur de JEU (GameApi) si ce joueur existe encore.
                // On utilise la factory pour créer un client qui pointe vers le bon port (5001)
                var gameClient = _factory.CreateClient("GameApi");
                
                var response = await gameClient.GetAsync($"api/players/{savedId.Value}");
                
                if (response.IsSuccessStatusCode)
                {
                    // TOUT EST OK : Le serveur nous reconnaît
                    CurrentPlayerId = savedId;
                    CurrentPlayerName = savedName;
                    NotifyStateChanged();
                }
                else
                {
                    // LE SERVEUR A OUBLIÉ (ex: redémarrage) : On nettoie tout
                    await Logout(); 
                }
            }
            catch
            {
                // Si le serveur est éteint ou injoignable, on déconnecte par sécurité
                await Logout();
            }
        }
    }

    public async Task<Player?> GetPlayerSession()
    {
        try 
        {
            // Utilise le client Auth (5200)
            return await _authHttp.GetFromJsonAsync<Player>("api/auth/session");
        }
        catch
        {
            return null;
        }
    }

    public async Task Login(int playerId, string playerName)
    {
        CurrentPlayerId = playerId;
        CurrentPlayerName = playerName;

        // Persistance
        await _localStorage.SetItemAsync("playerId", playerId);
        await _localStorage.SetItemAsync("playerName", playerName);

        NotifyStateChanged();
    }

    public async Task Logout()
    {
        CurrentPlayerId = null;
        CurrentPlayerName = null;

        // Nettoyage
        await _localStorage.RemoveItemAsync("playerId");
        await _localStorage.RemoveItemAsync("playerName");

        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}