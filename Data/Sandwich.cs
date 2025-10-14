namespace BackOfTheHouse.Data;

public class Sandwich
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool Toasted { get; set; }
}
