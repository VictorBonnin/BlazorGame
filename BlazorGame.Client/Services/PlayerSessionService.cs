using System.Net.Http.Json;
using System.Net.Http;
using SharedModels.Entities;
using Blazored.LocalStorage;
using System;
using System.Threading.Tasks;

namespace BlazorGame.Client.Services;

public class PlayerSessionService
{
    private readonly HttpClient _authHttp; 
    private readonly IHttpClientFactory _factory; 
    private readonly ILocalStorageService _localStorage;

    public PlayerSessionService(IHttpClientFactory factory, ILocalStorageService localStorage)
    {
        _factory = factory;
        _authHttp = factory.CreateClient("AuthApi"); 
        _localStorage = localStorage;
    }

    // 👇 [CORRECTION] : On met des 'set' publics et 'IsAuthenticated' devient une vraie variable
    public int? CurrentPlayerId { get; set; }
    public string? CurrentPlayerName { get; set; }
    public bool IsAuthenticated { get; set; } = false;
    
    public event Action? OnChange;

    public async Task Initialize()
    {
        // 1. On lit la mémoire du navigateur
        var savedId = await _localStorage.GetItemAsync<int?>("playerId");
        var savedName = await _localStorage.GetItemAsync<string?>("playerName");

        if (savedId.HasValue)
        {
            try 
            {
                // 2. Vérification auprès du serveur de JEU
                var gameClient = _factory.CreateClient("GameApi");
                var response = await gameClient.GetAsync($"api/players/{savedId.Value}");
                
                if (response.IsSuccessStatusCode)
                {
                    CurrentPlayerId = savedId;
                    CurrentPlayerName = savedName;
                    IsAuthenticated = true; // ✅ On valide l'auth ici
                    NotifyStateChanged();
                }
                else
                {
                    await Logout(); 
                }
            }
            catch
            {
                await Logout();
            }
        }
    }

    public async Task<Player?> GetPlayerSession()
    {
        try 
        {
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
        IsAuthenticated = true; // ✅ Mise à jour explicite

        // Persistance
        await _localStorage.SetItemAsync("playerId", playerId);
        await _localStorage.SetItemAsync("playerName", playerName);

        NotifyStateChanged();
    }

    public async Task Logout()
    {
        CurrentPlayerId = null;
        CurrentPlayerName = null;
        IsAuthenticated = false; // ✅ Mise à jour explicite

        // Nettoyage
        await _localStorage.RemoveItemAsync("playerId");
        await _localStorage.RemoveItemAsync("playerName");

        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}