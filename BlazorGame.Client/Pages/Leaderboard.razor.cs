using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace BlazorGame.Client.Pages;

// Rang, Libellé (on va afficher "Joueur #<id>"), Score total
public record PlayerRow(int Rank, string Name, int Score);

// DTO minimal qui correspond à la réponse de /api/leaderboard
file record LeaderItem(int Id, int Score, int PlayerId, DateTime? FinishedAt);

public partial class Leaderboard : ComponentBase
{
    [Inject] protected HttpClient Http { get; set; } = default!;

    protected bool loading;
    protected string? error;

    // TA liste existante : on la conserve pour compatibilité avec ton .razor
    protected List<PlayerRow> players = new();

    protected override async Task OnInitializedAsync()
    {
        loading = true; error = null;
        try
        {
            // Appel API (base address doit pointer sur GameServices, ex: http://localhost:5001)
            var data = await Http.GetFromJsonAsync<List<LeaderItem>>("/api/leaderboard") 
                       ?? new();

            // Tri décroissant (normalement déjà trié par l’API), puis projection vers PlayerRow
            players = data
                .OrderByDescending(x => x.Score)
                .Select((x, idx) =>
                    new PlayerRow(
                        Rank: idx + 1,
                        Name: $"Joueur #{x.PlayerId}", // si tu veux le vrai nom, il faudra joindre /api/players/{id}
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
