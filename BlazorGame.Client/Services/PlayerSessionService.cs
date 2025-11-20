namespace BlazorGame.Client.Services;

public class PlayerSessionService
{
    // ID du joueur actuellement connecté. Null si non connecté.
    public int? CurrentPlayerId { get; private set; }

    // Nom du joueur.
    public string? CurrentPlayerName { get; private set; }

    public bool IsAuthenticated => CurrentPlayerId.HasValue;

    public event Action? OnChange;

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