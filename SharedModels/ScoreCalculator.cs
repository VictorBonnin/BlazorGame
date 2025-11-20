namespace SharedModels;

using SharedModels.Entities;

public static class ScoreCalculator
{
    /// <summary>
    /// Calcule les points gagnés ou perdus pour une action spécifique dans une salle donnée.
    /// </summary>
    /// <param name="roomPlay">Les détails de l'action jouée dans la salle.</param>
    /// <returns>Le changement de score (positif, négatif ou nul).</returns>
    public static int CalculatePoints(RoomPlay roomPlay)
    {
        return (roomPlay.Type, roomPlay.Action) switch
        {
            // Salles de Combat
            (RoomType.Combat, PlayerAction.Combattre) => roomPlay.Difficulty * 10,
            (RoomType.Combat, PlayerAction.Fuir)      => -roomPlay.Difficulty * 2, // Pénalité légère pour la fuite
            
            // Salles de Butin (Loot)
            (RoomType.Loot,   PlayerAction.Fouiller)  => roomPlay.Difficulty * 5,
            (RoomType.Loot,   PlayerAction.Combattre) => -roomPlay.Difficulty, // Pénalité pour avoir essayé de se battre contre un coffre
            (RoomType.Loot,   PlayerAction.Fuir)      => 0, // Ne rien faire, pas de gain
            
            // Salles de Piège (Trap)
            (RoomType.Trap,   PlayerAction.Fuir)      => roomPlay.Difficulty * 3, // Récompense pour une évasion réussie (action par défaut)
            (RoomType.Trap,   PlayerAction.Combattre) => -roomPlay.Difficulty * 10, // Grosse pénalité pour avoir combattu le piège
            (RoomType.Trap,   PlayerAction.Fouiller)  => -roomPlay.Difficulty * 5, // Pénalité pour avoir fouillé un piège
            
            _ => 0
        };
    }
}