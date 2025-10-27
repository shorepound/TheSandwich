using BackOfTheHouse.Data.Scaffolded;
using BackOfTheHouse.Data;
using System;
using Microsoft.AspNetCore.Mvc;

namespace BackOfTheHouse.Controllers;

[ApiController]
[Route("api/builder")]
public class BuilderController : ControllerBase
{
    private readonly DockerSandwichContext? _docker;
    private readonly BackOfTheHouse.Data.SandwichContext? _sqlite;

    public BuilderController(IServiceProvider provider)
    {
        _docker = provider.GetService(typeof(DockerSandwichContext)) as DockerSandwichContext;
        _sqlite = provider.GetService(typeof(BackOfTheHouse.Data.SandwichContext)) as BackOfTheHouse.Data.SandwichContext;
    }

    public class BuilderDto
    {
        // Optional user-specified name for the sandwich
        public string? name { get; set; }
        public int? breadId { get; set; }
        // Whether the selected bread should be toasted
        public bool? toasted { get; set; }
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
        // Name is required
        var errors = new Dictionary<string, string>();
        if (string.IsNullOrWhiteSpace(dto.name))
        {
            errors["name"] = "Name is required";
            return BadRequest(new { errors });
        }

        // Validate IDs exist when provided
    string? bread = null;
    var cheeses = new List<string>();
    var dressings = new List<string>();
    var meats = new List<string>();

        if (dto.breadId.HasValue)
        {
            if (_docker != null)
            {
                var b = _docker.Breads.Find(dto.breadId.Value);
                if (b == null) errors["breadId"] = "Bread not found"; else bread = b.Name;
            }
            else if (_sqlite != null)
            {
                var b = _sqlite.Options.Find(dto.breadId.Value);
                if (b == null) errors["breadId"] = "Bread not found"; else bread = b.Name;
            }
        }

        if (dto.cheeseIds != null && dto.cheeseIds.Count > 0)
        {
            foreach (var cid in dto.cheeseIds)
            {
                if (_docker != null)
                {
                    var c = _docker.Cheeses.Find(cid);
                    if (c == null) { errors["cheeseIds"] = "One or more cheeses not found"; break; }
                    cheeses.Add(c.Name ?? "");
                }
                else if (_sqlite != null)
                {
                    var c = _sqlite.Options.Find(cid);
                    if (c == null) { errors["cheeseIds"] = "One or more cheeses not found"; break; }
                    cheeses.Add(c.Name ?? "");
                }
            }
        }

        if (dto.dressingIds != null && dto.dressingIds.Count > 0)
        {
            foreach (var did in dto.dressingIds)
            {
                if (_docker != null)
                {
                    var d = _docker.Dressings.Find(did);
                    if (d == null) { errors["dressingIds"] = "One or more dressings not found"; break; }
                    dressings.Add(d.Name ?? "");
                }
                else if (_sqlite != null)
                {
                    var d = _sqlite.Options.Find(did);
                    if (d == null) { errors["dressingIds"] = "One or more dressings not found"; break; }
                    dressings.Add(d.Name ?? "");
                }
            }
        }

        if (dto.meatIds != null && dto.meatIds.Count > 0)
        {
            foreach (var mid in dto.meatIds)
            {
                if (_docker != null)
                {
                    var m = _docker.Meats.Find(mid);
                    if (m == null) { errors["meatIds"] = "One or more meats not found"; break; }
                    meats.Add(m.Name ?? "");
                }
                else if (_sqlite != null)
                {
                    var m = _sqlite.Options.Find(mid);
                    if (m == null) { errors["meatIds"] = "One or more meats not found"; break; }
                    meats.Add(m.Name ?? "");
                }
            }
        }

        var toppings = new List<string>();
        if (dto.toppingIds != null && dto.toppingIds.Count > 0)
        {
            foreach (var tid in dto.toppingIds)
            {
                if (_docker != null)
                {
                    var t = _docker.Toppings.Find(tid);
                    if (t == null) { errors["toppingIds"] = "One or more toppings not found"; break; }
                    toppings.Add(t.Name ?? "");
                }
                else if (_sqlite != null)
                {
                    var t = _sqlite.Options.Find(tid);
                    if (t == null) { errors["toppingIds"] = "One or more toppings not found"; break; }
                    toppings.Add(t.Name ?? "");
                }
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
        if (!string.IsNullOrWhiteSpace(bread))
        {
            var btxt = bread + (dto.toasted.HasValue && dto.toasted.Value ? " (toasted)" : "");
            descParts.Add("Bread: " + btxt);
        }
    if (cheeses.Count > 0) descParts.Add("Cheese: " + string.Join(", ", cheeses.Where(s => !string.IsNullOrWhiteSpace(s))));
    if (dressings.Count > 0) descParts.Add("Dressing: " + string.Join(", ", dressings.Where(s => !string.IsNullOrWhiteSpace(s))));
    if (meats.Count > 0) descParts.Add("Meats: " + string.Join(", ", meats.Where(s => !string.IsNullOrWhiteSpace(s))));
    if (toppings.Count > 0) descParts.Add("Toppings: " + string.Join(", ", toppings.Where(s => !string.IsNullOrWhiteSpace(s))));
        var description = descParts.Count > 0 ? string.Join("; ", descParts) : null;

        // Determine user id from Authorization header (optional). We use the simple token format encoded by AuthService: base64(guid:userId:email:ticks)
        int? ownerUserId = null;
        try {
            var auth = Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(auth) && auth.StartsWith("Bearer "))
            {
                var token = auth.Substring("Bearer ".Length).Trim();
                try {
                    var raw = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(token));
                    var parts = raw.Split(':');
                    if (parts.Length >= 2 && int.TryParse(parts[1], out var uid)) ownerUserId = uid;
                } catch {}
            }
        } catch {}

        // Create sandwich in whichever context we have available
        if (_docker != null)
        {
        var sandwich = new BackOfTheHouse.Data.Scaffolded.Sandwich
            {
                Name = string.IsNullOrWhiteSpace(dto.name) ? name : dto.name,
                Description = description,
                    Price = dto.price,
            Toasted = dto.toasted ?? false,
            OwnerUserId = ownerUserId,
            IsPrivate = ownerUserId.HasValue
            };
            _docker.Sandwiches.Add(sandwich);
            _docker.SaveChanges();
            return CreatedAtAction(null, new { id = sandwich.Id }, sandwich);
        }
        else if (_sqlite != null)
        {
        var sandwich = new BackOfTheHouse.Data.Sandwich
            {
                Name = string.IsNullOrWhiteSpace(dto.name) ? name : dto.name,
                    Description = description ?? string.Empty,
                    Price = dto.price ?? 0.00m,
            Toasted = dto.toasted ?? false,
            OwnerUserId = ownerUserId,
            IsPrivate = ownerUserId.HasValue
            };
            _sqlite.Sandwiches.Add(sandwich);
            _sqlite.SaveChanges();
            return CreatedAtAction(null, new { id = sandwich.Id }, sandwich);
        }

    return StatusCode(500, new { error = "No database context available" });
    }
}
