namespace BackOfTheHouse.Data;

public class Sandwich
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool Toasted { get; set; }
    // Nullable owner id referencing dbo.tb_users.Id when available
    public int? OwnerUserId { get; set; }
    // Whether this sandwich is private (only editable by owner). Public sandwiches (created by guests) are editable by anyone.
    public bool IsPrivate { get; set; }
}
