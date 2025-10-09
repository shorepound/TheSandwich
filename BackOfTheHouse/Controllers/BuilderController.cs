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
        // allow multiple selections
        public List<int>? toppingIds { get; set; }
        public List<int>? cheeseIds { get; set; }
        public List<int>? dressingIds { get; set; }
        public List<int>? meatIds { get; set; }
        // optional price in dollars
        public decimal? price { get; set; }
        public string? note { get; set; }
    }

    [HttpPost]
    public IActionResult Post([FromBody] BuilderDto dto)
    {
        // Validate IDs exist when provided
    var errors = new Dictionary<string, string>();
    string? bread = null;
    var cheeses = new List<string>();
    var dressings = new List<string>();
    var meats = new List<string>();

        if (dto.breadId.HasValue)
        {
            var b = _db.Breads.Find(dto.breadId.Value);
            if (b == null) errors["breadId"] = "Bread not found"; else bread = b.Name;
        }

        if (dto.cheeseIds != null && dto.cheeseIds.Count > 0)
        {
            foreach (var cid in dto.cheeseIds)
            {
                var c = _db.Cheeses.Find(cid);
                if (c == null)
                {
                    errors["cheeseIds"] = "One or more cheeses not found";
                    break;
                }
                cheeses.Add(c.Name ?? "");
            }
        }

        if (dto.dressingIds != null && dto.dressingIds.Count > 0)
        {
            foreach (var did in dto.dressingIds)
            {
                var d = _db.Dressings.Find(did);
                if (d == null)
                {
                    errors["dressingIds"] = "One or more dressings not found";
                    break;
                }
                dressings.Add(d.Name ?? "");
            }
        }

        if (dto.meatIds != null && dto.meatIds.Count > 0)
        {
            foreach (var mid in dto.meatIds)
            {
                var m = _db.Meats.Find(mid);
                if (m == null)
                {
                    errors["meatIds"] = "One or more meats not found";
                    break;
                }
                meats.Add(m.Name ?? "");
            }
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
        if (meats.Count > 0) nameParts.Add(string.Join("/", meats));
        if (!string.IsNullOrWhiteSpace(bread)) nameParts.Add("on " + bread);
        var name = nameParts.Count > 0 ? string.Join(' ', nameParts) : "Custom Sandwich";

        var descParts = new List<string>();
        if (cheeses.Count > 0) descParts.Add("Cheese: " + string.Join(", ", cheeses.Where(s => !string.IsNullOrWhiteSpace(s))));
        if (dressings.Count > 0) descParts.Add("Dressing: " + string.Join(", ", dressings.Where(s => !string.IsNullOrWhiteSpace(s))));
        if (toppings.Count > 0) descParts.Add("Toppings: " + string.Join(", ", toppings.Where(s => !string.IsNullOrWhiteSpace(s))));
        var description = descParts.Count > 0 ? string.Join("; ", descParts) : null;

        var sandwich = new Sandwich
        {
            Name = name,
            Description = description,
            Price = dto.price
        };

        _db.Sandwiches.Add(sandwich);
        _db.SaveChanges();

        return CreatedAtAction(null, new { id = sandwich.Id }, sandwich);
    }
}
