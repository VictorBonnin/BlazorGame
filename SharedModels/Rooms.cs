using SharedModels.Entities; // <--- AJOUT IMPORTANT : Pour trouver Item et ItemType

namespace SharedModels;

// --- SUPPRESSION : On retire ItemType et Item car ils sont maintenant dans Entities/Item.cs ---

public class Monster
{
    public string Name { get; set; } = "Gobelin";
    public int Health { get; set; } = 10;
    public int Attack { get; set; } = 3;
}

// --- SUPPRESSION DE LA CLASSE ITEM ICI --- 

public enum RoomType { Trap, Combat, Loot, Shop, Exit, Sanctuary, Boss, Mystery } 
public enum PlayerAction { Combattre, Fouiller, UtiliserObjet, Fuir } 

public class Room
{
    public int Id { get; set; }
    public RoomType Type { get; set; }
    public int Difficulty { get; set; }
    
    public string Description { get; set; } = "Une salle sombre.";
    public List<Monster> Monsters { get; set; } = new List<Monster>();
    
    // Ici, il utilisera SharedModels.Entities.Item grâce au 'using'
    public List<Item> Loot { get; set; } = new List<Item>();

    public bool IsCleared { get; set; } = false; 
}

// NOTE : Cette classe PlayerState semble être un doublon de celle dans GameDtos.cs (Client).
// Si tu ne l'utilises pas côté Serveur, tu pourrais la supprimer, mais laissons-la pour l'instant.
public class PlayerState
{
    public int Health { get; set; } = 100;
    public int MaxHealth { get; set; } = 100;
    public int AttackPower { get; set; } = 10;
    public int Gold { get; set; } = 0;
    public List<Item> Inventory { get; set; } = new List<Item>();
}

public class RoomPlay
{
    public int Id { get; set; } 
    public int Index { get; set; }
    public RoomType Type { get; set; }
    public int Difficulty { get; set; }
    public PlayerAction Action { get; set; }
    public int Points { get; set; }
}