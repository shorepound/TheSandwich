using BackOfTheHouse.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configure application services
builder.Services.AddSandwichDatabase(builder.Configuration);
builder.Services.AddCorsPolicy();
// Application services (passes IConfiguration so services like email can be configured)
builder.Services.AddApplicationServices(builder.Configuration);
// Data protection (used for protecting MFA secrets)
builder.Services.AddDataProtection();

var app = builder.Build();

// Apply pending EF Core migrations for SQLite SandwichContext (if used) and ensure seed data.
using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	try
	{
		var sqlite = services.GetService<BackOfTheHouse.Data.SandwichContext>();
		if (sqlite != null)
		{
			// Apply any pending migrations (no-op if none)
			sqlite.Database.Migrate();
		}
	}
	catch (Exception ex)
	{
		var logger = services.GetService<ILoggerFactory>()?.CreateLogger("Startup");
		logger?.LogWarning(ex, "Automatic migration failed during startup");
	}

	// Ensure database is seeded for SQLite scenarios
	app.EnsureDatabaseSeeded(builder.Configuration);
}

// Configure request pipeline
app.UseMiddleware<CachingMiddleware>();
app.ConfigureRequestPipeline();

app.Run();
