namespace SharedModels.Entities;

public class Adventure
{
    public int Id { get; set; }

    // FK -> Player
    public int PlayerId { get; set; }
    public Player? Player { get; set; }

    // Métadonnées
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FinishedAt { get; set; }
    public int Score { get; set; }

    // Historique des salles jouées (owned collection EF)
    public List<RoomPlay> Rooms { get; set; } = new();
}
