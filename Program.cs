using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// EF Core DbContext - prefer SQLite for cross-platform local development
builder.Services.AddDbContext<BackOfTheHouse.Data.SandwichContext>(options =>
{
    // Use a file-based SQLite DB for local/dev to avoid LocalDB platform issues
    var conn = builder.Configuration.GetValue<string>("SqliteConnection") ?? "Data Source=Data/sandwich.db";
    options.UseSqlite(conn);
});

// CORS policy for local dev (if you want to call Kestrel directly from ng serve)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalDev", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Ensure DB created and seed sample data
BackOfTheHouse.Data.SandwichContext.EnsureSeedData(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Serve static files (for production Angular build served from wwwroot)
app.UseStaticFiles();

app.UseRouting();

// If you choose not to use the Angular proxy in development, enable CORS
app.UseCors("AllowLocalDev");

app.UseAuthorization();
 
app.MapControllers();

// Keep the existing minimal weather endpoint for backward compatibility
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// Fallback to serve SPA index.html when no other route matches (for client-side routing)
app.MapFallbackToFile("index.html");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
