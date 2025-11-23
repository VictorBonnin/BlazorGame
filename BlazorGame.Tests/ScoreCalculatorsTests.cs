using SharedModels;
using SharedModels.Entities;

namespace BlazorGame.Tests;

public class ScoreCalculatorsTests
{
    [Theory]
    [InlineData(RoomType.Combat,   PlayerAction.Combattre, 3, 30)] // Combat réussi
    [InlineData(RoomType.Combat,   PlayerAction.Fuir,      3, 15)] // Fuite d'un combat
    [InlineData(RoomType.Combat,   PlayerAction.Fouiller,  3, 0)]  // Fouiller en combat (neutre)
    
    [InlineData(RoomType.Loot,     PlayerAction.Fouiller,  4, 20)] // Butin réussi
    [InlineData(RoomType.Loot,     PlayerAction.Combattre, 4, -4)] // Combattre butin (pénalité)
    [InlineData(RoomType.Loot,     PlayerAction.Fuir,      4, 0)]  // Fuir le butin (neutre)

    [InlineData(RoomType.Trap,     PlayerAction.Fuir,      5, 15)] // Esquive de piège (récompense)
    [InlineData(RoomType.Trap,     PlayerAction.Fouiller,  5, -25)]// Fouiller un piège (grosse pénalité)
    [InlineData(RoomType.Trap,     PlayerAction.Combattre, 5, -50)]// Combattre un piège (max pénalité)

    [InlineData(RoomType.Combat,   PlayerAction.Combattre, 1, 10)] // Difficulté minimale
    [InlineData(RoomType.Combat,   PlayerAction.Combattre, 5, 50)] // Difficulté maximale

    public void CalculatePoints_ShouldReturnCorrectScoreChange(
        RoomType roomType, 
        PlayerAction action, 
        int difficulty, 
        int expectedPoints)
    {
        // Arrange
        var roomPlay = new RoomPlay
        {
            Index = 1,
            Type = roomType,
            Difficulty = difficulty, // Utilisation de la nouvelle propriété
            Action = action,
            Points = 0 // Cette propriété sera mise à jour par le service appelant si besoin
        };

        // Act
        int actualPoints = ScoreCalculator.CalculatePoints(roomPlay);

        // Assert
        Assert.Equal(expectedPoints, actualPoints);
    }
}