using GameServices.Controllers;
using GameServices.Data;
using GameServices.Logic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Entities;

namespace BlazorGame.Tests;

public class AdventuresControllerTests
{
    private GameDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new GameDbContext(options);
        return context;
    }

    [Fact]
    public async Task StartAdventure_ReturnsCreated_WhenPlayerExists()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var player = new Player { Id = 1, UserName = "Victor" };
        context.Players.Add(player);
        await context.SaveChangesAsync();

        var controller = new AdventuresController(context);
        var dto = new StartAdventureDto(1, 3, 5); //

        // Act
        var result = await controller.StartAdventure(dto);

        // Assert
        var actionResult = Assert.IsType<CreatedResult>(result.Result);
        var payload = Assert.IsType<StartPayload>(actionResult.Value);
        
        Assert.NotNull(payload.Adventure);
        Assert.Equal(1, payload.Adventure.PlayerId);
        Assert.NotEmpty(payload.Rooms);
    }
}