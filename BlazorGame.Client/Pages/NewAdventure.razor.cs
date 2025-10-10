using Microsoft.AspNetCore.Components;
using SharedModels;
using System.Net.Http.Json;

namespace BlazorGame.Client.Pages;

public partial class NewAdventure : ComponentBase
{
    protected bool loading, started, finished;
    protected List<Room> rooms = new();
    protected int current, score;
    protected string? error;

    [Inject] protected HttpClient Http { get; set; } = default!;

    protected async Task StartAdventure()
    {
        loading = true; finished = false; error = null; score = 0; current = 0;
        try
        {
            rooms = await Http.GetFromJsonAsync<List<Room>>("http://localhost:5001/api/dungeon/new")
                    ?? new();
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

    protected void Choose(string action)
    {
        score = ScoreCalculator.Apply(action, score);
        current++;
        if (current >= rooms.Count) { started = false; finished = true; }
        StateHasChanged();
    }
}
