using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BackOfTheHouse.Services;

public class NoopEmailService : IEmailService
{
    private readonly ILogger<NoopEmailService> _logger;
    public NoopEmailService(ILogger<NoopEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendWelcomeAsync(string toEmail, string? displayName = null)
    {
        _logger.LogInformation("NoopEmailService: would send welcome email to {email}", toEmail);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string toEmail, string resetUrl)
    {
        _logger.LogInformation("NoopEmailService: would send password reset to {email} with url {url}", toEmail, resetUrl);
        return Task.CompletedTask;
    }
}
