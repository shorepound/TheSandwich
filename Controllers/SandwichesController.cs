using BackOfTheHouse.Data;
using BackOfTheHouse.Data.Scaffolded;
using Microsoft.AspNetCore.Mvc;

namespace BackOfTheHouse.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SandwichesController : ControllerBase
{
    private readonly DockerSandwichContext _ctx;

    public SandwichesController(DockerSandwichContext ctx)
    {
        _ctx = ctx;
    }

    [HttpGet]
    public IEnumerable<BackOfTheHouse.Data.Scaffolded.Sandwich> Get()
    {
        return _ctx.Sandwiches.ToList();
    }

    [HttpGet("{id}")]
    public ActionResult<BackOfTheHouse.Data.Scaffolded.Sandwich> Get(int id)
    {
        var s = _ctx.Sandwiches.Find(id);
        if (s == null) return NotFound();
        return s;
    }
}
