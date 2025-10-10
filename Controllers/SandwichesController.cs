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
            _docker.SaveChanges();
            return NoContent();
        }
        if (_sqlite != null)
        {
            var s = _sqlite.Sandwiches.Find(id);
            if (s == null) return NotFound();
            _sqlite.Sandwiches.Remove(s);
            _sqlite.SaveChanges();
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
    }

    [HttpPut("{id}")]
    public ActionResult Update(int id, [FromBody] UpdateDto dto)
    {
        if (_docker != null)
        {
            var s = _docker.Sandwiches.Find(id);
            if (s == null) return NotFound();
            if (dto.name != null) s.Name = dto.name;
            if (dto.description != null) s.Description = dto.description;
            if (dto.price.HasValue) s.Price = dto.price.Value;
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
