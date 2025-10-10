using BackOfTheHouse.Data;
using BackOfTheHouse.Data.Scaffolded;
using Microsoft.AspNetCore.Mvc;

namespace BackOfTheHouse.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SandwichesController : ControllerBase
{
    private readonly DockerSandwichContext? _docker;
    private readonly BackOfTheHouse.Data.SandwichContext? _sqlite;

    public SandwichesController(IServiceProvider provider)
    {
        // Resolve optional DbContexts from the provider. Using GetService<T>() allows
        // the controller to be created even when one of the contexts isn't registered.
        _docker = provider.GetService(typeof(DockerSandwichContext)) as DockerSandwichContext;
        _sqlite = provider.GetService(typeof(BackOfTheHouse.Data.SandwichContext)) as BackOfTheHouse.Data.SandwichContext;
    }

    public class SandwichDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal? Price { get; set; }
    }

    private static SandwichDto ToDto(BackOfTheHouse.Data.Scaffolded.Sandwich s)
    {
        return new SandwichDto { Id = s.Id, Name = s.Name ?? string.Empty, Description = s.Description, Price = s.Price };
    }

    private static SandwichDto ToDto(BackOfTheHouse.Data.Sandwich s)
    {
        return new SandwichDto { Id = s.Id, Name = s.Name ?? string.Empty, Description = s.Description, Price = s.Price };
    }

    [HttpGet]
    public ActionResult<IEnumerable<SandwichDto>> Get()
    {
        if (_docker != null)
        {
            return Ok(_docker.Sandwiches.Select(s => ToDto(s)).ToList());
        }
        if (_sqlite != null)
        {
            return Ok(_sqlite.Sandwiches.Select(s => ToDto(s)).ToList());
        }
        return Ok(Array.Empty<SandwichDto>());
    }

    [HttpGet("{id}")]
    public ActionResult<SandwichDto> Get(int id)
    {
        if (_docker != null)
        {
            var s = _docker.Sandwiches.Find(id);
            if (s == null) return NotFound();
            return ToDto(s);
        }
        if (_sqlite != null)
        {
            var s = _sqlite.Sandwiches.Find(id);
            if (s == null) return NotFound();
            return ToDto(s);
        }
        return NotFound();
    }

    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        if (_docker != null)
        {
            var s = _docker.Sandwiches.Find(id);
            if (s == null) return NotFound();
            _docker.Sandwiches.Remove(s);
                if (_docker != null) _docker.SaveChanges();
            return NoContent();
        }
        if (_sqlite != null)
        {
            var s = _sqlite.Sandwiches.Find(id);
            if (s == null) return NotFound();
            _sqlite.Sandwiches.Remove(s);
            if (_sqlite != null) _sqlite.SaveChanges();
            return NoContent();
        }
        return NotFound();
    }

    // POST /api/sandwiches/backfill-prices
    // Sets any NULL prices to 0.00 to provide a consistent display value. Only applies to Docker-scaffolded context where Price is nullable.
    [HttpPost("backfill-prices")]
    public ActionResult BackfillPrices()
    {
        int updated = 0;
        if (_docker != null)
        {
            var list = _docker.Sandwiches.Where(s => s.Price == null).ToList();
            foreach (var s in list) { s.Price = 0.00m; }
            _docker.SaveChanges();
            updated = list.Count;
        }
        // For sqlite context Price is non-nullable in our model; no-op
        return Ok(new { updated });
    }

    public class UpdateDto
    {
        public string? name { get; set; }
        public string? description { get; set; }
        public decimal? price { get; set; }
        // Optional composition fields - if provided we'll rebuild the description
        public int? breadId { get; set; }
        public bool? toasted { get; set; }
        public List<int>? cheeseIds { get; set; }
        public List<int>? dressingIds { get; set; }
        public List<int>? meatIds { get; set; }
        public List<int>? toppingIds { get; set; }
    }

    [HttpPut("{id}")]
    public ActionResult Update(int id, [FromBody] UpdateDto dto)
    {
        if (_docker != null)
        {
            var s = _docker.Sandwiches.Find(id);
            if (s == null) return NotFound();
            if (dto.name != null) s.Name = dto.name;
            if (dto.price.HasValue) s.Price = dto.price.Value;

            // If caller provided an explicit description, prefer it. Otherwise,
            // if any composition fields are present, rebuild the description
            if (dto.description != null)
            {
                s.Description = dto.description;
            }
            else if (dto.breadId.HasValue || dto.cheeseIds != null || dto.dressingIds != null || dto.meatIds != null || dto.toppingIds != null)
            {
                var breads = new List<string>();
                var cheeses = new List<string>();
                var dressings = new List<string>();
                var meats = new List<string>();
                var toppings = new List<string>();
                string? bread = null;
                if (_docker != null)
                {
                    if (dto.breadId.HasValue) { var b = _docker.Breads.Find(dto.breadId.Value); if (b != null) bread = b.Name; }
                    if (dto.cheeseIds != null) foreach (var idc in dto.cheeseIds) { var c = _docker.Cheeses.Find(idc); if (c != null) cheeses.Add(c.Name ?? ""); }
                    if (dto.dressingIds != null) foreach (var idd in dto.dressingIds) { var d = _docker.Dressings.Find(idd); if (d != null) dressings.Add(d.Name ?? ""); }
                    if (dto.meatIds != null) foreach (var idm in dto.meatIds) { var m = _docker.Meats.Find(idm); if (m != null) meats.Add(m.Name ?? ""); }
                    if (dto.toppingIds != null) foreach (var idt in dto.toppingIds) { var t = _docker.Toppings.Find(idt); if (t != null) toppings.Add(t.Name ?? ""); }
                }
                else if (_sqlite != null)
                {
                    if (dto.breadId.HasValue) { var b = _sqlite.Options.Find(dto.breadId.Value); if (b != null) bread = b.Name; }
                    if (dto.cheeseIds != null) foreach (var idc in dto.cheeseIds) { var c = _sqlite.Options.Find(idc); if (c != null) cheeses.Add(c.Name ?? ""); }
                    if (dto.dressingIds != null) foreach (var idd in dto.dressingIds) { var d = _sqlite.Options.Find(idd); if (d != null) dressings.Add(d.Name ?? ""); }
                    if (dto.meatIds != null) foreach (var idm in dto.meatIds) { var m = _sqlite.Options.Find(idm); if (m != null) meats.Add(m.Name ?? ""); }
                    if (dto.toppingIds != null) foreach (var idt in dto.toppingIds) { var t = _sqlite.Options.Find(idt); if (t != null) toppings.Add(t.Name ?? ""); }
                }

                var parts = new List<string>();
                if (!string.IsNullOrWhiteSpace(bread)) { var btxt = bread + (dto.toasted.HasValue && dto.toasted.Value ? " (toasted)" : ""); parts.Add("Bread: " + btxt); }
                if (cheeses.Count > 0) parts.Add("Cheese: " + string.Join(", ", cheeses.Where(s1 => !string.IsNullOrWhiteSpace(s1))));
                if (dressings.Count > 0) parts.Add("Dressing: " + string.Join(", ", dressings.Where(s1 => !string.IsNullOrWhiteSpace(s1))));
                if (meats.Count > 0) parts.Add("Meats: " + string.Join(", ", meats.Where(s1 => !string.IsNullOrWhiteSpace(s1))));
                if (toppings.Count > 0) parts.Add("Toppings: " + string.Join(", ", toppings.Where(s1 => !string.IsNullOrWhiteSpace(s1))));
                s.Description = parts.Count > 0 ? string.Join("; ", parts) : s.Description;
            }
            _docker.SaveChanges();
            return NoContent();
        }
        if (_sqlite != null)
        {
            var s = _sqlite.Sandwiches.Find(id);
            if (s == null) return NotFound();
            if (dto.name != null) s.Name = dto.name;
            if (dto.description != null) s.Description = dto.description;
            if (dto.price.HasValue) s.Price = dto.price.Value;
            _sqlite.SaveChanges();
            return NoContent();
        }
        return NotFound();
    }
}
