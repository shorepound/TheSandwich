using System.Threading.Tasks;

namespace BackOfTheHouse.Services;

public interface IEmailService
{
    /// <summary>
    /// Sends a welcome email to the specified address. Implementations should be
    /// resilient — if email sending is not configured this can be a no-op.
    /// </summary>
    Task SendWelcomeAsync(string toEmail, string? displayName = null);
}
