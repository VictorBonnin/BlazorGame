using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using SharedModels;                 // ← pour ScoreCalculator
using SharedModels.Entities;        // ← pour Room/RoomPlay/RoomType
using BlazorGame.Client.Services;   // ← pour StartRequest/StartPayload/FinishRequest

namespace BlazorGame.Client.Pages;

public partial class NewAdventure : ComponentBase
{
    protected bool loading, started, finished;
    protected List<Room> rooms = new();
    protected List<RoomPlay> plays = new();
    protected int current, score, adventureId;
    protected string? error;

    [Inject] protected HttpClient Http { get; set; } = default!;

    // Lance l’aventure via l’API V2
    protected async Task StartAdventure()
    {
        loading = true; finished = false; error = null; score = 0; current = 0;
        plays.Clear();

        try
        {
            // TODO: remonter le vrai PlayerId; pour l’instant démo = 1
            var req = new StartRequest(PlayerId: 1, MinRooms: 3, MaxRooms: 5);
            var res = await Http.PostAsJsonAsync("/api/adventures/start", req);
            res.EnsureSuccessStatusCode();

            var payload = await res.Content.ReadFromJsonAsync<StartPayload>();
            adventureId = payload?.AdventureId ?? 0;
            rooms = payload?.Rooms ?? new();

            started = rooms.Count > 0;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            started = false;
        }
        finally
        {
            loading = false;
            StateHasChanged();
        }
    }

    // Choix dans une salle (doit retourner Task pour être awaitable côté .razor)
    protected Task Choose(string action)
    {
        if (rooms is null || rooms.Count == 0 || finished) 
            return Task.CompletedTask;

        // scoring basique (conserve ta logique)
        score = ScoreCalculator.Apply(action, score);

        // On log la salle jouée (utile pour /finish)
        var room = rooms[current];
        var act = action.ToLowerInvariant() switch
        {
            "combattre" => PlayerAction.Combattre,
            "fuir"      => PlayerAction.Fuir,
            "fouiller"  => PlayerAction.Fouiller,
            _           => PlayerAction.Fouiller
        };
        plays.Add(new RoomPlay
        {
            Id        = current + 1,     // identifiant technique local
            Index     = room.Index,
            Type      = room.Type,
            Action    = act,
            Points    = score            // ou les points gagnés sur CETTE salle si tu les distinctes
        });

        current++;

        if (current >= rooms.Count)
        {
            // on peut finir en asynchrone (pas obligatoire d’attendre)
            _ = FinishAdventure();
            started = false;
            finished = true;
        }

        StateHasChanged();
        return Task.CompletedTask;
    }

    private async Task FinishAdventure()
    {
        try
        {
            var req = new FinishRequest(score, plays);
            var res = await Http.PostAsJsonAsync($"/api/adventures/{adventureId}/finish", req);
            res.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            error = $"Fin non sauvegardée: {ex.Message}";
        }
    }
}
