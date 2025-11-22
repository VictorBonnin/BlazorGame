using SharedModels.Entities;
using SharedModels; // Nécessaire pour Room et RoomPlay

namespace BlazorGame.Client.Services;

// DTO pour l'état local du joueur (C'est ici qu'on stocke les données pendant la partie)
public class PlayerState
{
    public int Hp { get; set; } = 100;
    public int Score { get; set; } = 0;

    public List<Item> Inventory { get; set; } = new(); 
    // -------------------------------

    public bool IsDead => Hp <= 0;
}

// Requêtes / réponses utilisées par le client pour appeler l'API
public record StartRequest(int PlayerId, int? MinRooms, int? MaxRooms);
public record StartPayload(int AdventureId, List<Room> Rooms);
public record FinishRequest(int Score, List<RoomPlay> Rooms);