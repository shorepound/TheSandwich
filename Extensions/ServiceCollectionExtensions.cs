using Microsoft.EntityFrameworkCore;
using BackOfTheHouse.Data;
using BackOfTheHouse.Data.Scaffolded;

namespace BackOfTheHouse.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSandwichDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var dockerConn = Environment.GetEnvironmentVariable("DOCKER_DB_CONNECTION") 
                         ?? configuration.GetValue<string>("DockerConnection");
        
        if (!string.IsNullOrEmpty(dockerConn))
        {
            // Use scaffolded DockerSandwichContext to connect to the real SQL Server
            services.AddDbContext<DockerSandwichContext>(options => 
                options.UseSqlServer(dockerConn));
        }
        else
        {
            // Use a file-based SQLite DB for local/dev to avoid LocalDB platform issues
            var sqliteConn = configuration.GetValue<string>("SqliteConnection") 
                           ?? "Data Source=Data/sandwich.db";
            services.AddDbContext<SandwichContext>(options => 
                options.UseSqlite(sqliteConn));
        }

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Auth service depends on the DbContext and Data Protection; scoped lifetime is appropriate.
        services.AddScoped<BackOfTheHouse.Services.IAuthService, BackOfTheHouse.Services.AuthService>();
        // Email service: register an SMTP-backed sender if Smtp:Host configured, otherwise a no-op implementation
        var smtpHost = configuration.GetValue<string>("Smtp:Host");
        if (!string.IsNullOrEmpty(smtpHost))
        {
            services.AddSingleton<BackOfTheHouse.Services.IEmailService, BackOfTheHouse.Services.SmtpEmailService>();
        }
        else
        {
            services.AddSingleton<BackOfTheHouse.Services.IEmailService, BackOfTheHouse.Services.NoopEmailService>();
        }
        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        // CORS policy for local dev (if you want to call Kestrel directly from ng serve)
        services.AddCors(options =>
        {
            options.AddPolicy("AllowLocalDev", policy =>
            {
                policy.WithOrigins("http://localhost:4200")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        return services;
    }

    public static void EnsureDatabaseSeeded(this WebApplication app, IConfiguration configuration)
    {
        // If we're using SQLite (no DOCKER_DB_CONNECTION), ensure DB created and seed sample data
        var dockerConnCheck = Environment.GetEnvironmentVariable("DOCKER_DB_CONNECTION") 
                             ?? configuration.GetValue<string>("DockerConnection");
        
        if (string.IsNullOrEmpty(dockerConnCheck))
        {
            SandwichContext.EnsureSeedData(app.Services);
        }
    }

    public static void ConfigureRequestPipeline(this WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            // In development we want detailed exceptions and the OpenAPI UI.
            // Do NOT enable automatic HTTPS redirection in development so the
            // Angular dev server can proxy to the Kestrel HTTP endpoint without
            // receiving 307 redirects.
            app.UseDeveloperExceptionPage();
            app.MapOpenApi();
        }
        else
        {
            // In non-development environments we redirect HTTP -> HTTPS.
            app.UseHttpsRedirection();
        }

        // Serve static files (for production Angular build served from wwwroot)
        app.UseStaticFiles();

        app.UseRouting();

        // If you choose not to use the Angular proxy in development, enable CORS
        app.UseCors("AllowLocalDev");

        app.UseAuthorization();
 
        app.MapControllers();

        // Fallback to serve SPA index.html when no other route matches (for client-side routing)
        app.MapFallbackToFile("index.html");
    }
}