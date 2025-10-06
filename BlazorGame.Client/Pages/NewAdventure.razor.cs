using Microsoft.AspNetCore.Components;

namespace BlazorGame.Client.Pages;

public partial class NewAdventure : ComponentBase
{
    private bool isStarted;
    protected void StartAdventure() => isStarted = true;
}
