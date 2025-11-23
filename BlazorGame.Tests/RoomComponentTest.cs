using BlazorGame.Client.Pages;
using Bunit;
using SharedModels;
using SharedModels.Entities;

namespace BlazorGame.Tests;

public class RoomComponentTests : TestContext
{
    // 1. On change "void" par "async Task" pour pouvoir utiliser await
    [Fact]
    public async Task UseItem_Potion_HealsPlayerAndRemovesItem()
    {
        // Arrange
        var potion = new Item { Name = "Potion de Vie", Type = ItemType.Potion, EffectPower = 20 };
        var playerState = new PlayerState 
        { 
            Health = 50, 
            Inventory = new List<Item> { potion } 
        };

        var cut = RenderComponent<RoomComponent>(parameters => parameters
            .Add(p => p.CurrentPlayerState, playerState)
        );

        // Act
        // CORRECTION : On utilise InvokeAsync pour dire à Blazor "Exécute ça sur ton thread d'UI"
        await cut.InvokeAsync(() => cut.Instance.UseItem(potion));

        // Assert
        Assert.Equal(70, playerState.Health);
        Assert.DoesNotContain(potion, playerState.Inventory);
    }

    // 2. Idem ici : async Task
    [Fact]
    public async Task UseItem_Potion_DoesNotExceedMaxHealth()
    {
        // Arrange
        var potion = new Item { Name = "Potion", Type = ItemType.Potion, EffectPower = 50 };
        var playerState = new PlayerState 
        { 
            Health = 90, 
            Inventory = new List<Item> { potion } 
        };

        var cut = RenderComponent<RoomComponent>(parameters => parameters
            .Add(p => p.CurrentPlayerState, playerState)
        );

        // Act
        // CORRECTION : InvokeAsync ici aussi
        await cut.InvokeAsync(() => cut.Instance.UseItem(potion));

        // Assert
        Assert.Equal(100, playerState.Health);
    }
}