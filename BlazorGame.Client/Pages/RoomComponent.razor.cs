using Microsoft.AspNetCore.Components;
using SharedModels.Entities;

namespace BlazorGame.Client.Pages;

public partial class RoomComponent : ComponentBase
{
    [Parameter]
    public Room Room { get; set; } = default!;

    [Parameter]
    public EventCallback<PlayerAction> OnActionSelected { get; set; }

    [Parameter]
    public bool IsLoading { get; set; }

    private async Task ChooseAction(PlayerAction action)
    {
        await OnActionSelected.InvokeAsync(action);
    }
}