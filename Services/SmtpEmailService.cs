using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BackOfTheHouse.Services;

public class SmtpEmailService : IEmailService
{
    private readonly ILogger<SmtpEmailService> _logger;
    private readonly SmtpOptions _opts;

    public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger)
    {
        _logger = logger;
        _opts = config.GetSection("Smtp").Get<SmtpOptions>() ?? new SmtpOptions();
    }

    public Task SendWelcomeAsync(string toEmail, string? displayName = null)
    {
        if (string.IsNullOrEmpty(_opts.Host))
        {
            _logger.LogInformation("SMTP not configured - skipping welcome email to {email}", toEmail);
            return Task.CompletedTask;
        }

        try
        {
            var from = new MailAddress(_opts.FromAddress ?? "no-reply@example.com", _opts.FromName ?? "The Sandwich");
            var to = new MailAddress(toEmail, displayName ?? string.Empty);

            using var msg = new MailMessage(from, to)
            {
                Subject = "Welcome to The Sandwich",
                IsBodyHtml = true,
                Body = BuildWelcomeBody(displayName)
            };

            using var client = new SmtpClient(_opts.Host, _opts.Port ?? 25)
            {
                EnableSsl = _opts.UseSsl,
            };

            if (!string.IsNullOrEmpty(_opts.Username))
            {
                client.Credentials = new NetworkCredential(_opts.Username, _opts.Password);
            }

            // Send synchronously inside Task.Run to avoid blocking callers
            return Task.Run(() => client.Send(msg));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {email}", toEmail);
            return Task.CompletedTask;
        }
    }

    private string BuildWelcomeBody(string? name)
    {
        var n = string.IsNullOrWhiteSpace(name) ? "there" : name;
        return $@"<div style='font-family:system-ui, -apple-system, Segoe UI, Roboto, Helvetica, Arial; color:#222'>
<h2 style='color:#06629f'>Welcome, {System.Net.WebUtility.HtmlEncode(n)}!</h2>
<p>Thanks for registering at <strong>The Sandwich</strong>. You're all set — head over to <a href='{_opts.PublicUrl ?? "http://localhost:4200"}'>the app</a> to build and browse sandwiches.</p>
<p>— The Sandwich team</p>
</div>";
    }

    private class SmtpOptions
    {
        public string? Host { get; set; }
        public int? Port { get; set; }
        public bool UseSsl { get; set; } = true;
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? FromAddress { get; set; }
        public string? FromName { get; set; }
        public string? PublicUrl { get; set; }
    }
}
