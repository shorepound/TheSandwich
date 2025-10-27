using System.Threading.Tasks;

namespace BackOfTheHouse.Services;

public interface IEmailService
{
    /// <summary>
    /// Sends a welcome email to the specified address. Implementations should be
    /// resilient — if email sending is not configured this can be a no-op.
    /// </summary>
    Task SendWelcomeAsync(string toEmail, string? displayName = null);

    /// <summary>
    /// Send a password-reset email to the specified address containing a reset link.
    /// Implementations may be no-ops in development if SMTP is not configured.
    /// </summary>
    Task SendPasswordResetAsync(string toEmail, string resetUrl);
}
