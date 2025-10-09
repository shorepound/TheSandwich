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

    // POST /api/sandwiches/backfill-prices
    // Sets any NULL prices to 0.00 to provide a consistent display value.
    [HttpPost("backfill-prices")]
    public ActionResult BackfillPrices()
    {
        var list = _ctx.Sandwiches.Where(s => s.Price == null).ToList();
        foreach (var s in list)
        {
            s.Price = 0.00m;
        }
        _ctx.SaveChanges();
        return Ok(new { updated = list.Count });
    }
}
