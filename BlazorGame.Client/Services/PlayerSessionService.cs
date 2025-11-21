using System.Net.Http.Json; // Nécessaire pour GetFromJsonAsync
using System.Net.Http;      // Nécessaire pour IHttpClientFactory
using SharedModels.Entities; // Pour l'objet Player

namespace BlazorGame.Client.Services;

public class PlayerSessionService
{
    private readonly HttpClient _http;

    // On injecte la "Fabrique" (Factory) pour choisir la bonne adresse
    public PlayerSessionService(IHttpClientFactory factory)
    {
        // On choisit explicitement le canal vers le port 5200
        _http = factory.CreateClient("AuthApi");
    }

    public int? CurrentPlayerId { get; private set; }
    public string? CurrentPlayerName { get; private set; }
    public bool IsAuthenticated => CurrentPlayerId.HasValue;
    public event Action? OnChange;

    // Cette méthode appelle http://localhost:5200/api/auth/session
    public async Task<Player?> GetPlayerSession()
    {
        try 
        {
            return await _http.GetFromJsonAsync<Player>("api/auth/session");
        }
        catch
        {
            return null; // Si erreur, on considère que le joueur n'est pas connecté
        }
    }

    public void Login(int playerId, string playerName)
    {
        CurrentPlayerId = playerId;
        CurrentPlayerName = playerName;
        NotifyStateChanged();
    }

    public void Logout()
    {
        CurrentPlayerId = null;
        CurrentPlayerName = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}