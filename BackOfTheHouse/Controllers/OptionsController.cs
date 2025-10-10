using BackOfTheHouse.Data.Scaffolded;
using Microsoft.AspNetCore.Mvc;

namespace BackOfTheHouse.Controllers;

[ApiController]
[Route("api/options")]
public class OptionsController : ControllerBase
{
    private readonly DockerSandwichContext? _docker;
    private readonly BackOfTheHouse.Data.SandwichContext? _sqlite;

    // Make the Docker context optional. In environments where DOCKER_DB_CONNECTION
    // isn't provided we register the SQLite `SandwichContext` instead and the
    // options tables (breads, cheeses, etc.) don't exist. Return a 503 in that
    // case so the frontend can surface a clear error instead of the app failing
    // to start due to DI resolution errors.
    public OptionsController(DockerSandwichContext? docker = null, BackOfTheHouse.Data.SandwichContext? sqlite = null)
    {
        _docker = docker;
        _sqlite = sqlite;
    }

    private static object ToOption(int id, string? name) => new { id, label = name };

    [HttpGet("breads")]
    public IActionResult Breads()
    {
        if (_docker != null)
        {
            var list = _docker.Breads.Select(b => ToOption(b.Id, b.Name)).ToList();
            return Ok(list);
        }
        if (_sqlite != null)
        {
            var list = _sqlite.Options
                .Where(o => o.Category == "breads")
                .Select(o => ToOption(o.Id, o.Name))
                .ToList();
            return Ok(list);
        }
        return Ok(Array.Empty<object>());
    }

    [HttpGet("cheeses")]
    public IActionResult Cheeses()
    {
        if (_docker != null)
        {
            var list = _docker.Cheeses.Select(c => ToOption(c.Id, c.Name)).ToList();
            return Ok(list);
        }
        if (_sqlite != null)
        {
            var list = _sqlite.Options
                .Where(o => o.Category == "cheeses")
                .Select(o => ToOption(o.Id, o.Name))
                .ToList();
            return Ok(list);
        }
        return Ok(Array.Empty<object>());
    }

    [HttpGet("dressings")]
    public IActionResult Dressings()
    {
        if (_docker != null)
        {
            var list = _docker.Dressings.Select(d => ToOption(d.Id, d.Name)).ToList();
            return Ok(list);
        }
        if (_sqlite != null)
        {
            var list = _sqlite.Options
                .Where(o => o.Category == "dressings")
                .Select(o => ToOption(o.Id, o.Name))
                .ToList();
            return Ok(list);
        }
        return Ok(Array.Empty<object>());
    }

    [HttpGet("meats")]
    public IActionResult Meats()
    {
        if (_docker != null)
        {
            var list = _docker.Meats.Select(m => ToOption(m.Id, m.Name)).ToList();
            return Ok(list);
        }
        if (_sqlite != null)
        {
            var list = _sqlite.Options
                .Where(o => o.Category == "meats")
                .Select(o => ToOption(o.Id, o.Name))
                .ToList();
            return Ok(list);
        }
        return Ok(Array.Empty<object>());
    }

    [HttpGet("toppings")]
    public IActionResult Toppings()
    {
        if (_docker != null)
        {
            var list = _docker.Toppings.Select(t => ToOption(t.Id, t.Name)).ToList();
            return Ok(list);
        }
        if (_sqlite != null)
        {
            var list = _sqlite.Options
                .Where(o => o.Category == "toppings")
                .Select(o => ToOption(o.Id, o.Name))
                .ToList();
            return Ok(list);
        }
        return Ok(Array.Empty<object>());
    }
}
