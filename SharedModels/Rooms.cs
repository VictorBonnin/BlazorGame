namespace SharedModels.Entities;

public enum RoomType { Combat, Loot, Trap }
public enum PlayerAction { Combattre, Fuir, Fouiller }

public record Room(int Index, RoomType Type, int Difficulty);

// ← LA seule définition de RoomPlay (supprime toute autre copie)
public class RoomPlay
{
    // Clé technique pour EF OwnsMany
    public int Id { get; set; }

    public int Index { get; set; }
    public RoomType Type { get; set; }
    public PlayerAction Action { get; set; }
    public int Points { get; set; }
}
