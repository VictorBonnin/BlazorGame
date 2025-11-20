namespace SharedModels;

// Nouveaux types d'entités pour peupler les salles
public class Monster
{
    public string Name { get; set; } = "Gobelin";
    public int Health { get; set; } = 10;
    public int Attack { get; set; } = 3;
}

public class Item
{
    public string Name { get; set; } = "Pièce d'Or";
    public string Type { get; set; } = "Gold"; // e.g., Weapon, Gold, Potion
    public int Value { get; set; } = 1; // Quantité ou valeur monétaire
}

// CORRECTION: Enumérations mises à jour pour correspondre à ScoreCalculator.cs
public enum RoomType { Trap, Combat, Loot, Shop, Exit } 
public enum PlayerAction { Combattre, Fouiller, UtiliserObjet, Fuir } 

// Mise à jour de la classe Room pour inclure le contenu généré
public class Room
{
    public int Id { get; set; }
    public RoomType Type { get; set; }
    public int Difficulty { get; set; }
    
    // CORRECTION: Ancien EventDescription est maintenant Description
    public string Description { get; set; } = "Une salle sombre.";
    public List<Monster> Monsters { get; set; } = new List<Monster>();
    public List<Item> Loot { get; set; } = new List<Item>();
}

public class RoomPlay
{
    public int Id { get; set; } // Nécessaire pour EF Core (ajouté dans l'étape précédente)
    public int Index { get; set; }
    public RoomType Type { get; set; }
    public int Difficulty { get; set; }
    public PlayerAction Action { get; set; }
    public int Points { get; set; }
}