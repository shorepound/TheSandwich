using BackOfTheHouse.Data;
using Microsoft.AspNetCore.Mvc;

namespace BackOfTheHouse.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SandwichesController : ControllerBase
{
    private readonly SandwichContext _ctx;

    public SandwichesController(SandwichContext ctx)
    {
        _ctx = ctx;
    }

    [HttpGet]
    public IEnumerable<Sandwich> Get()
    {
        return _ctx.Sandwiches.ToList();
    }

    [HttpGet("{id}")]
    public ActionResult<Sandwich> Get(int id)
    {
        var s = _ctx.Sandwiches.Find(id);
        if (s == null) return NotFound();
        return s;
    }
}
