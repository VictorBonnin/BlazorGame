using Microsoft.AspNetCore.Components;

namespace BlazorGame.Client.Pages;

public record PlayerRow(int Id, string Name, int Score);

public partial class Leaderboard : ComponentBase
{
    protected List<PlayerRow> players = new()
    {
        new(1,"Aventurier_1",120),
        new(2,"Aventurier_2",95),
        new(3,"Aventurier_3",60),
    };
}
