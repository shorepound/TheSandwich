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
