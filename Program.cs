using BackOfTheHouse.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configure application services
builder.Services.AddSandwichDatabase(builder.Configuration);
builder.Services.AddCorsPolicy();
// Application services
builder.Services.AddApplicationServices();
// Data protection (used for protecting MFA secrets)
builder.Services.AddDataProtection();

var app = builder.Build();

// Ensure database is seeded for SQLite scenarios
app.EnsureDatabaseSeeded(builder.Configuration);

// Configure request pipeline
app.UseMiddleware<CachingMiddleware>();
app.ConfigureRequestPipeline();

app.Run();
