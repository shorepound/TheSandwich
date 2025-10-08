using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    [HttpGet]
    public IEnumerable<object> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new
        {
            date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            temperatureC = Random.Shared.Next(-20, 55),
            summary = Summaries[Random.Shared.Next(Summaries.Length)]
        });
    }
}
