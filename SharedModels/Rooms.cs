namespace SharedModels;

public enum RoomType { Combat, Loot, Trap }
public record Room(int Index, RoomType Type, int Difficulty);
