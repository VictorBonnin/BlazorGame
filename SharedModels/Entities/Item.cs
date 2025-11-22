namespace SharedModels.Entities;

public enum ItemType 
{ 
    Treasure, 
    Potion, 
    Weapon, 
    Gold,     
    Artifact  
}

public class Item
{
    public string Name { get; set; } = string.Empty;
    public ItemType Type { get; set; }
    
    public int EffectPower { get; set; } 
    
    public int ScoreValue { get; set; } 
    public string Description { get; set; } = ""; 
}