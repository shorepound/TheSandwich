using System;
using System.Collections.Generic;

namespace BackOfTheHouse.Data.Scaffolded;

public partial class Sandwich
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal? Price { get; set; }
    public bool Toasted { get; set; }
    public int? OwnerUserId { get; set; }
    public bool IsPrivate { get; set; }
}
