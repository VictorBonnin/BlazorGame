namespace SharedModels;

using SharedModels.Entities;

public static class ScoreCalculator
{
    public const int FleeScore = 15; 

    public static int CalculatePoints(RoomPlay roomPlay)
    {
        return (roomPlay.Type, roomPlay.Action) switch
        {
            // Salles de Combat classiques
            (RoomType.Combat, PlayerAction.Combattre) => roomPlay.Difficulty * 10,
            (RoomType.Combat, PlayerAction.Fuir)      => FleeScore, 
            
            // --- Salles de Boss ---
            // Le Boss donne plus de points (ex: x20 au lieu de x10)
            (RoomType.Boss,   PlayerAction.Combattre) => roomPlay.Difficulty * 20, 
            (RoomType.Boss,   PlayerAction.Fuir)      => FleeScore,
            
            // Salles de Butin (Loot)
            (RoomType.Loot,   PlayerAction.Fouiller)  => roomPlay.Difficulty * 5,
            (RoomType.Loot,   PlayerAction.Combattre) => -roomPlay.Difficulty,
            (RoomType.Loot,   PlayerAction.Fuir)      => 0,
            
            // Salles de Piège (Trap)
            (RoomType.Trap,   PlayerAction.Fuir)      => roomPlay.Difficulty * 3,
            (RoomType.Trap,   PlayerAction.Combattre) => -roomPlay.Difficulty * 10,
            (RoomType.Trap,   PlayerAction.Fouiller)  => -roomPlay.Difficulty * 5,
            
            _ => 0
        };
    }
}