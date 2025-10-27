using BackOfTheHouse.Data;
using BackOfTheHouse.Data.Scaffolded;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public bool Toasted { get; set; }

        // Composition fields inferred from Description (or stored later if schema changes)
        public int? BreadId { get; set; }
        public List<int>? CheeseIds { get; set; }
        public List<int>? DressingIds { get; set; }
        public List<int>? MeatIds { get; set; }
        public List<int>? ToppingIds { get; set; }
    }

    private static SandwichDto ToDto(BackOfTheHouse.Data.Scaffolded.Sandwich s, DockerSandwichContext? docker = null)
    {
        // Build a basic dto and attempt to infer composition ids by looking up option names
        var dto = new SandwichDto { Id = s.Id, Name = s.Name ?? string.Empty, Description = s.Description, Price = s.Price, Toasted = s.Toasted };
        if (!string.IsNullOrWhiteSpace(s.Description) && docker != null)
        {
            // Parse server-side description in the same simple format the frontend expects: "Bread: X (toasted); Cheese: A, B; Dressing: C; Meats: ...; Toppings: ..."
            try
            {
                var parts = s.Description.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToList();
                foreach (var part in parts)
                {
                    if (part.StartsWith("Bread:", StringComparison.OrdinalIgnoreCase))
                    {
                        var btxt = part.Substring(6).Trim();
                        // strip toasted suffix if present
                        var toasted = false;
                        if (btxt.EndsWith("(toasted)", StringComparison.OrdinalIgnoreCase))
                        {
                            toasted = true;
                            btxt = btxt.Substring(0, btxt.Length - "(toasted)".Length).Trim();
                        }
                        // find matching bread id
                        var breads = docker.Breads;
                        if (breads != null)
                        {
                            var b = breads.AsEnumerable().FirstOrDefault(x => x.Name != null && x.Name.Equals(btxt, StringComparison.OrdinalIgnoreCase));
                            if (b != null) dto.BreadId = b.Id;
                        }
                        if (toasted) dto.Toasted = true;
                    }
                    else if (part.StartsWith("Cheese:", StringComparison.OrdinalIgnoreCase))
                    {
                        var list = part.Substring(7).Split(',').Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
                        var ids = new List<int>();
                        foreach (var name in list)
                        {
                            var cheeses = docker.Cheeses;
                            if (cheeses != null)
                            {
                                var c = cheeses.AsEnumerable().FirstOrDefault(x => x.Name != null && x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                                if (c != null) ids.Add(c.Id);
                            }
                        }
                        if (ids.Count > 0) dto.CheeseIds = ids;
                    }
                    else if (part.StartsWith("Dressing:", StringComparison.OrdinalIgnoreCase))
                    {
                        var list = part.Substring(9).Split(',').Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
                        var ids = new List<int>();
                        foreach (var name in list)
                        {
                            var dressings = docker.Dressings;
                            if (dressings != null)
                            {
                                var d = dressings.AsEnumerable().FirstOrDefault(x => x.Name != null && x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                                if (d != null) ids.Add(d.Id);
                            }
                        }
                        if (ids.Count > 0) dto.DressingIds = ids;
                    }
                    else if (part.StartsWith("Meats:", StringComparison.OrdinalIgnoreCase) || part.StartsWith("Meat:", StringComparison.OrdinalIgnoreCase))
                    {
                        var idx = part.IndexOf(':');
                        var list = part.Substring(idx + 1).Split(',').Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
                        var ids = new List<int>();
                        foreach (var name in list)
                        {
                            var meats = docker.Meats;
                            if (meats != null)
                            {
                                var m = meats.AsEnumerable().FirstOrDefault(x => x.Name != null && x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                                if (m != null) ids.Add(m.Id);
                            }
                        }
                        if (ids.Count > 0) dto.MeatIds = ids;
                    }
                    else if (part.StartsWith("Toppings:", StringComparison.OrdinalIgnoreCase) || part.StartsWith("Topping:", StringComparison.OrdinalIgnoreCase))
                    {
                        var idx = part.IndexOf(':');
                        var list = part.Substring(idx + 1).Split(',').Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
                        var ids = new List<int>();
                        foreach (var name in list)
                        {
                            var toppings = docker.Toppings;
                            if (toppings != null)
                            {
                                var t = toppings.AsEnumerable().FirstOrDefault(x => x.Name != null && x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                                if (t != null) ids.Add(t.Id);
                            }
                        }
                        if (ids.Count > 0) dto.ToppingIds = ids;
                    }
                }
            }
            catch { /* be resilient - don't fail the API if parsing has issues */ }
        }
        return dto;
    }

    private static SandwichDto ToDto(BackOfTheHouse.Data.Sandwich s, BackOfTheHouse.Data.SandwichContext? sqlite = null)
    {
        // SQLite-backed Sandwich model uses a different context; reuse the same inference logic
        var dto = new SandwichDto { Id = s.Id, Name = s.Name ?? string.Empty, Description = s.Description, Price = s.Price, Toasted = s.Toasted };
        if (!string.IsNullOrWhiteSpace(s.Description) && sqlite != null)
        {
            try
            {
                var parts = s.Description.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToList();
                foreach (var part in parts)
                {
                    if (part.StartsWith("Bread:", StringComparison.OrdinalIgnoreCase))
                    {
                        var btxt = part.Substring(6).Trim();
                        var toasted = false;
                        if (btxt.EndsWith("(toasted)", StringComparison.OrdinalIgnoreCase))
                        {
                            toasted = true;
                            btxt = btxt.Substring(0, btxt.Length - "(toasted)".Length).Trim();
                        }
                        var opt = sqlite.Options.FirstOrDefault(x => x.Category == "breads" && x.Name != null && x.Name.Equals(btxt, StringComparison.OrdinalIgnoreCase));
                        if (opt != null) dto.BreadId = opt.Id;
                        if (toasted) dto.Toasted = true;
                    }
                    else if (part.StartsWith("Cheese:", StringComparison.OrdinalIgnoreCase))
                    {
                        var list = part.Substring(7).Split(',').Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
                        var ids = new List<int>();
                        foreach (var name in list)
                        {
                            var opt = sqlite.Options.FirstOrDefault(x => x.Category == "cheeses" && x.Name != null && x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                            if (opt != null) ids.Add(opt.Id);
                        }
                        if (ids.Count > 0) dto.CheeseIds = ids;
                    }
                    else if (part.StartsWith("Dressing:", StringComparison.OrdinalIgnoreCase))
                    {
                        var list = part.Substring(9).Split(',').Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
                        var ids = new List<int>();
                        foreach (var name in list)
                        {
                            var opt = sqlite.Options.FirstOrDefault(x => x.Category == "dressings" && x.Name != null && x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                            if (opt != null) ids.Add(opt.Id);
                        }
                        if (ids.Count > 0) dto.DressingIds = ids;
                    }
                    else if (part.StartsWith("Meats:", StringComparison.OrdinalIgnoreCase) || part.StartsWith("Meat:", StringComparison.OrdinalIgnoreCase))
                    {
                        var idx = part.IndexOf(':');
                        var list = part.Substring(idx + 1).Split(',').Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
                        var ids = new List<int>();
                        foreach (var name in list)
                        {
                            var opt = sqlite.Options.FirstOrDefault(x => x.Category == "meats" && x.Name != null && x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                            if (opt != null) ids.Add(opt.Id);
                        }
                        if (ids.Count > 0) dto.MeatIds = ids;
                    }
                    else if (part.StartsWith("Toppings:", StringComparison.OrdinalIgnoreCase) || part.StartsWith("Topping:", StringComparison.OrdinalIgnoreCase))
                    {
                        var idx = part.IndexOf(':');
                        var list = part.Substring(idx + 1).Split(',').Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
                        var ids = new List<int>();
                        foreach (var name in list)
                        {
                            var opt = sqlite.Options.FirstOrDefault(x => x.Category == "toppings" && x.Name != null && x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                            if (opt != null) ids.Add(opt.Id);
                        }
                        if (ids.Count > 0) dto.ToppingIds = ids;
                    }
                }
            }
            catch { }
        }
        return dto;
    }    [HttpGet]
    public ActionResult<IEnumerable<SandwichDto>> Get()
    {
        if (_docker != null)
        {
            var sandwiches = _docker.Sandwiches.ToList();
            return Ok(sandwiches.Select(s => ToDto(s, _docker)).ToList());
        }
        if (_sqlite != null)
        {
            var sandwiches = _sqlite.Sandwiches.ToList();
            return Ok(sandwiches.Select(s => ToDto(s, _sqlite)).ToList());
        }
        return Ok(Array.Empty<SandwichDto>());
    }

    // GET /api/sandwiches/mine
    [HttpGet("mine")]
    public ActionResult<IEnumerable<SandwichDto>> Mine()
    {
        // Try to parse the simple token format: base64(guid:userId:email:ticks)
        int? userId = null;
        try {
            var auth = Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(auth) && auth.StartsWith("Bearer "))
            {
                var token = auth.Substring("Bearer ".Length).Trim();
                try {
                    var raw = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(token));
                    var parts = raw.Split(':');
                    if (parts.Length >= 2 && int.TryParse(parts[1], out var uid)) userId = uid;
                } catch {}
            }
        } catch {}

        if (userId == null)
        {
            // If we couldn't determine the user, return empty list to avoid leaking data
            return Ok(Array.Empty<SandwichDto>());
        }

        if (_docker != null)
        {
            var sandwiches = _docker.Sandwiches.Where(s => EF.Property<int?>(s, "OwnerUserId") == userId).ToList();
            return Ok(sandwiches.Select(s => ToDto(s, _docker)).ToList());
        }
        if (_sqlite != null)
        {
            var sandwiches = _sqlite.Sandwiches.Where(s => s.OwnerUserId == userId).ToList();
            return Ok(sandwiches.Select(s => ToDto(s, _sqlite)).ToList());
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
            return ToDto(s, _docker);
        }
        if (_sqlite != null)
        {
            var s = _sqlite.Sandwiches.Find(id);
            if (s == null) return NotFound();
            return ToDto(s, _sqlite);
        }
        return NotFound();
    }

    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var userId = ParseUserIdFromAuthorization();
        if (_docker != null)
        {
            var s = _docker.Sandwiches.Find(id);
            if (s == null) return NotFound();
            // If sandwich has an owner, only the owner may delete it
            try {
                var owner = (int?) (s.GetType().GetProperty("OwnerUserId")?.GetValue(s));
                var isPrivate = false;
                try { isPrivate = (bool)(s.GetType().GetProperty("IsPrivate")?.GetValue(s) ?? false); } catch {}
                // If sandwich is private and has an owner, only owner may delete
                if (isPrivate && owner.HasValue && userId != null && owner.Value != userId.Value) return Forbid();
            } catch {}
            _docker.Sandwiches.Remove(s);
            _docker!.SaveChanges();
            return NoContent();
        }
        if (_sqlite != null)
        {
            var s = _sqlite.Sandwiches.Find(id);
            if (s == null) return NotFound();
            if (s.IsPrivate && s.OwnerUserId.HasValue && userId != null && s.OwnerUserId.Value != userId.Value) return Forbid();
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
            _docker!.SaveChanges();
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
        var userId = ParseUserIdFromAuthorization();
        if (_docker != null)
        {
            var s = _docker.Sandwiches.Find(id);
            if (s == null) return NotFound();
            try {
                var owner = (int?) (s.GetType().GetProperty("OwnerUserId")?.GetValue(s));
                var isPrivate = false;
                try { isPrivate = (bool)(s.GetType().GetProperty("IsPrivate")?.GetValue(s) ?? false); } catch {}
                if (isPrivate && owner.HasValue && userId != null && owner.Value != userId.Value) return Forbid();
            } catch {}
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
            // persist Toasted if provided
            if (dto.toasted.HasValue) s.Toasted = dto.toasted.Value;
            _docker!.SaveChanges();
            return NoContent();
        }
        if (_sqlite != null)
        {
            var s = _sqlite.Sandwiches.Find(id);
            if (s == null) return NotFound();
            if (s.IsPrivate && s.OwnerUserId.HasValue && userId != null && s.OwnerUserId.Value != userId.Value) return Forbid();
            if (dto.name != null) s.Name = dto.name;
            if (dto.description != null) s.Description = dto.description;
            if (dto.price.HasValue) s.Price = dto.price.Value;
            if (dto.toasted.HasValue) s.Toasted = dto.toasted.Value;
            _sqlite.SaveChanges();
            return NoContent();
        }
        return NotFound();
    }

    private int? ParseUserIdFromAuthorization()
    {
        try
        {
            var auth = Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(auth) && auth.StartsWith("Bearer "))
            {
                var token = auth.Substring("Bearer ".Length).Trim();
                try
                {
                    var raw = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(token));
                    var parts = raw.Split(':');
                    if (parts.Length >= 2 && int.TryParse(parts[1], out var uid)) return uid;
                }
                catch { }
            }
        }
        catch { }
        return null;
    }
}
