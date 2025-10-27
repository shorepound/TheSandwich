using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BackOfTheHouse.Services;

namespace BackOfTheHouse.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService auth, ILogger<AuthController> logger)
    {
        _auth = auth;
        _logger = logger;
    }

    public class LoginDto { public string? email { get; set; } public string? password { get; set; } }
    public class RegisterDto { public string? email { get; set; } public string? password { get; set; } }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (dto?.email == null || dto?.password == null) return BadRequest(new { error = "email and password required" });
        var (ok, err) = await _auth.RegisterAsync(dto.email, dto.password);
        if (!ok)
        {
            // Map duplicate email case to 409 Conflict so clients can act (e.g., offer login)
            if (string.Equals(err, "email already registered", System.StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Registration attempt for existing email: {email}", dto.email);
                return Conflict(new { error = "email already registered" });
            }
            return BadRequest(new { error = err });
        }

        return Ok(new { success = true });
    }

    [HttpGet("exists")]
    public async Task<IActionResult> Exists([FromQuery] string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return BadRequest(new { error = "email required" });
        var exists = await _auth.EmailExistsAsync(email);
        return Ok(new { exists });
    }

    public class ForgotDto { public string? email { get; set; } }

    [HttpPost("forgot")]
    public async Task<IActionResult> Forgot([FromBody] ForgotDto dto)
    {
        if (dto?.email == null) return BadRequest(new { error = "email required" });

        // Log incoming forgot requests so we can trace browser-originated calls in dev
        _logger.LogInformation("Forgot endpoint invoked for email: {email}", dto?.email);

        // Don't reveal whether an email exists — respond with success in all cases.
        try
        {
            var email = dto.email!.Trim();
            var exists = await _auth.EmailExistsAsync(email);
            if (!exists)
            {
                // Always return OK to avoid leaking which addresses are registered.
                return Ok(new { success = true });
            }

            // Generate a one-time token (GUID) and construct a reset URL.
            var token = Guid.NewGuid().ToString("D");
            // Use configured public URL if available, otherwise assume frontend at http://localhost:4200
            var publicUrl = Request.Headers.ContainsKey("Origin") ? Request.Headers["Origin"].ToString() : null;
            if (string.IsNullOrEmpty(publicUrl)) publicUrl = "http://localhost:4200";
            var resetUrl = $"{publicUrl.TrimEnd('/')}/reset-password?token={System.Net.WebUtility.UrlEncode(token)}";

            // Send reset email (no-op if email not configured)
            var emailSvc = HttpContext.RequestServices.GetService(typeof(BackOfTheHouse.Services.IEmailService)) as BackOfTheHouse.Services.IEmailService;
            if (emailSvc != null)
            {
                try {
                    await emailSvc.SendPasswordResetAsync(email, resetUrl);
                } catch (Exception ex) {
                    _logger.LogWarning(ex, "Failed to send password reset email to {email}", email);
                }
            }

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Forgot password handling failed");
            return StatusCode(500, new { error = "server error" });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        _logger.LogInformation("Login attempt received for email: {email}", dto?.email);
        if (dto?.email == null || dto?.password == null)
        {
            _logger.LogWarning("Login attempt with missing credentials");
            return BadRequest(new { error = "email and password required" });
        }
        var (ok, status, token, requiresMfa, mfaToken, error) = await _auth.AuthenticateAsync(dto.email, dto.password);
        if (!ok)
        {
            _logger.LogWarning("Login failed with status {status}: {error}", status, error);
            if (status == 401) return new ObjectResult(new { error = "Invalid credentials" }) { StatusCode = 401 };
            return StatusCode(status, new { error = error ?? "Login failed" });
        }

        if (requiresMfa)
        {
            return Ok(new { requiresMfa = true, mfaToken });
        }

        return Ok(new { token });
    }
}
