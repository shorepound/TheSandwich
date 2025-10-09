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
        // allow multiple toppings
        public List<int>? toppingIds { get; set; }
    }

    [HttpPost]
    public IActionResult Post([FromBody] BuilderDto dto)
    {
    // Validate IDs exist when provided
    var errors = new Dictionary<string, string>();
    string? bread = null, cheese = null, dressing = null, meat = null;

        if (dto.breadId.HasValue)
        {
            var b = _db.Breads.Find(dto.breadId.Value);
            if (b == null) errors["breadId"] = "Bread not found"; else bread = b.Name;
        }
        if (dto.cheeseId.HasValue)
        {
            var c = _db.Cheeses.Find(dto.cheeseId.Value);
            if (c == null) errors["cheeseId"] = "Cheese not found"; else cheese = c.Name;
        }
        if (dto.dressingId.HasValue)
        {
            var d = _db.Dressings.Find(dto.dressingId.Value);
            if (d == null) errors["dressingId"] = "Dressing not found"; else dressing = d.Name;
        }
        if (dto.meatId.HasValue)
        {
            var m = _db.Meats.Find(dto.meatId.Value);
            if (m == null) errors["meatId"] = "Meat not found"; else meat = m.Name;
        }
        var toppings = new List<string>();
        if (dto.toppingIds != null && dto.toppingIds.Count > 0)
        {
            foreach (var tid in dto.toppingIds)
            {
                var t = _db.Toppings.Find(tid);
                if (t == null)
                {
                    errors["toppingIds"] = "One or more toppings not found";
                    break;
                }
                toppings.Add(t.Name ?? "");
            }
        }

        if (errors.Count > 0)
        {
            return BadRequest(new { errors });
        }

        var nameParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(meat)) nameParts.Add(meat!);
        if (!string.IsNullOrWhiteSpace(bread)) nameParts.Add("on " + bread);
        var name = nameParts.Count > 0 ? string.Join(' ', nameParts) : "Custom Sandwich";

        var descParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(cheese)) descParts.Add("Cheese: " + cheese);
        if (!string.IsNullOrWhiteSpace(dressing)) descParts.Add("Dressing: " + dressing);
    if (toppings.Count > 0) descParts.Add("Toppings: " + string.Join(", ", toppings.Where(s => !string.IsNullOrWhiteSpace(s))));
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
