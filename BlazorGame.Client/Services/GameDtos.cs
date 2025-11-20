using SharedModels.Entities;
using SharedModels; // <--- CORRECTION CS0246: Ajout pour Room et RoomPlay

namespace BlazorGame.Client.Services;

// Requêtes / réponses utilisées par le client pour appeler l'API
public record StartRequest(int PlayerId, int? MinRooms, int? MaxRooms);
public record StartPayload(int AdventureId, List<Room> Rooms);

public record FinishRequest(int Score, List<RoomPlay> Rooms);