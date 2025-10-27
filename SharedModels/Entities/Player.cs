namespace SharedModels.Entities;

public class Player
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;

    public List<Adventure> Adventures { get; set; } = new();
}
