using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace BlazorGame.Client.Pages;

// ViewModel pour l'affichage
public record PlayerRow(int Rank, string Name, int Score);

// DTOs pour lire la réponse JSON de l'API
// On ajoute "LeaderPlayer" pour capter l'objet "player" envoyé par le serveur
file record LeaderPlayer(string UserName);
file record LeaderItem(int Id, int Score, int PlayerId, DateTime? FinishedAt, LeaderPlayer? Player);

public partial class Leaderboard : ComponentBase
{
    [Inject] protected HttpClient Http { get; set; } = default!;

    protected bool loading;
    protected string? error;
    protected List<PlayerRow> players = new();

    protected override async Task OnInitializedAsync()
    {
        loading = true; error = null;
        try
        {
            // Appel API
            var data = await Http.GetFromJsonAsync<List<LeaderItem>>("/api/leaderboard") 
                       ?? new();

            // Construction de la liste pour l'affichage
            players = data
                .OrderByDescending(x => x.Score)
                .Select((x, idx) =>
                    new PlayerRow(
                        Rank: idx + 1,
                        // CORRECTION ICI : On prend le UserName s'il existe, sinon on met l'ID
                        Name: x.Player?.UserName ?? $"Joueur #{x.PlayerId}", 
                        Score: x.Score))
                .ToList();
        }
        catch (Exception ex)
        {
            error = $"Impossible de charger le classement : {ex.Message}";
        }
        finally
        {
            loading = false;
        }
    }
}