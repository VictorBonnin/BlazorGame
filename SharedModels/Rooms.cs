namespace SharedModels;

// NOUVEAU : Enumération pour identifier facilement le type d'objet
public enum ItemType { Gold, Potion, Weapon, Armor, Artifact }

public class Monster
{
    public string Name { get; set; } = "Gobelin";
    public int Health { get; set; } = 10;
    public int Attack { get; set; } = 3;
}

public class Item
{
    public string Name { get; set; } = "Pièce d'Or";
    
    // MODIFICATION : On utilise l'enum au lieu d'un string pour plus de sécurité
    public ItemType Type { get; set; } = ItemType.Gold; 
    
    public int Value { get; set; } = 1; // Valeur en or ou score
    
    // NOUVEAU : La puissance de l'effet (ex: +20 PV, +5 Attaque)
    public int EffectPower { get; set; } 
    
    // NOUVEAU : Petite description pour l'interface
    public string Description { get; set; } = ""; 
}

// MODIFICATION : Ajout de Sanctuary, Boss et Mystery
public enum RoomType { Trap, Combat, Loot, Shop, Exit, Sanctuary, Boss, Mystery } 

public enum PlayerAction { Combattre, Fouiller, UtiliserObjet, Fuir } 

public class Room
{
    public int Id { get; set; }
    public RoomType Type { get; set; }
    public int Difficulty { get; set; }
    
    public string Description { get; set; } = "Une salle sombre.";
    public List<Monster> Monsters { get; set; } = new List<Monster>();
    public List<Item> Loot { get; set; } = new List<Item>();

    // NOUVEAU : Pour savoir si le joueur a déjà vidé la salle
    public bool IsCleared { get; set; } = false; 
}

// NOUVEAU : Cette classe servira à suivre l'état du héros pendant la partie (Côté Client)
public class PlayerState
{
    public int Health { get; set; } = 100;
    public int MaxHealth { get; set; } = 100;
    public int AttackPower { get; set; } = 10;
    public int Gold { get; set; } = 0;
    public List<Item> Inventory { get; set; } = new List<Item>();
}

// Classe utilisée pour l'historique des parties (Base de données)
public class RoomPlay
{
    public int Id { get; set; } 
    public int Index { get; set; }
    public RoomType Type { get; set; }
    public int Difficulty { get; set; }
    public PlayerAction Action { get; set; }
    public int Points { get; set; }
}