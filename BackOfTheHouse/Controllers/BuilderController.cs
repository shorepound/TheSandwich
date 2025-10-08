using BackOfTheHouse.Data.Scaffolded;
using Microsoft.AspNetCore.Mvc;

namespace BackOfTheHouse.Controllers;

[ApiController]
[Route("api/builder")]
public class BuilderController : ControllerBase
{
    private readonly DockerSandwichContext _db;

    public BuilderController(DockerSandwichContext db)
    {
        _db = db;
    }

    public class BuilderDto
    {
        public int? breadId { get; set; }
        public int? cheeseId { get; set; }
        public int? dressingId { get; set; }
        public int? meatId { get; set; }
        public int? toppingId { get; set; }
    }

    [HttpPost]
    public IActionResult Post([FromBody] BuilderDto dto)
    {
        // lookup names (null-safe)
        string? bread = dto.breadId.HasValue ? _db.Breads.Find(dto.breadId.Value)?.Name : null;
        string? cheese = dto.cheeseId.HasValue ? _db.Cheeses.Find(dto.cheeseId.Value)?.Name : null;
        string? dressing = dto.dressingId.HasValue ? _db.Dressings.Find(dto.dressingId.Value)?.Name : null;
        string? meat = dto.meatId.HasValue ? _db.Meats.Find(dto.meatId.Value)?.Name : null;
        string? topping = dto.toppingId.HasValue ? _db.Toppings.Find(dto.toppingId.Value)?.Name : null;

        var nameParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(meat)) nameParts.Add(meat!);
        if (!string.IsNullOrWhiteSpace(bread)) nameParts.Add("on " + bread);
        var name = nameParts.Count > 0 ? string.Join(' ', nameParts) : "Custom Sandwich";

        var descParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(cheese)) descParts.Add("Cheese: " + cheese);
        if (!string.IsNullOrWhiteSpace(dressing)) descParts.Add("Dressing: " + dressing);
        if (!string.IsNullOrWhiteSpace(topping)) descParts.Add("Topping: " + topping);
        var description = descParts.Count > 0 ? string.Join("; ", descParts) : null;

        var sandwich = new Sandwich
        {
            Name = name,
            Description = description,
            Price = null
        };

        _db.Sandwiches.Add(sandwich);
        _db.SaveChanges();

        return CreatedAtAction(null, new { id = sandwich.Id }, sandwich);
    }
}
