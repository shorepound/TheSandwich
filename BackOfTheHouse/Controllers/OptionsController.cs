using BackOfTheHouse.Data.Scaffolded;
using Microsoft.AspNetCore.Mvc;

namespace BackOfTheHouse.Controllers;

[ApiController]
[Route("api/options")]
public class OptionsController : ControllerBase
{
    private readonly DockerSandwichContext _docker;
    private readonly BackOfTheHouse.Data.SandwichContext? _sqlite;

    public OptionsController(DockerSandwichContext docker, BackOfTheHouse.Data.SandwichContext? sqlite = null)
    {
        _docker = docker;
        _sqlite = sqlite;
    }

    private static object ToOption(int id, string? name) => new { id, label = name };

    [HttpGet("breads")]
    public IActionResult Breads()
    {
        var list = _docker.Breads.Select(b => ToOption(b.Id, b.Name)).ToList();
        return Ok(list);
    }

    [HttpGet("cheeses")]
    public IActionResult Cheeses()
    {
        var list = _docker.Cheeses.Select(c => ToOption(c.Id, c.Name)).ToList();
        return Ok(list);
    }

    [HttpGet("dressings")]
    public IActionResult Dressings()
    {
        var list = _docker.Dressings.Select(d => ToOption(d.Id, d.Name)).ToList();
        return Ok(list);
    }

    [HttpGet("meats")]
    public IActionResult Meats()
    {
        var list = _docker.Meats.Select(m => ToOption(m.Id, m.Name)).ToList();
        return Ok(list);
    }

    [HttpGet("toppings")]
    public IActionResult Toppings()
    {
        var list = _docker.Toppings.Select(t => ToOption(t.Id, t.Name)).ToList();
        return Ok(list);
    }
}
