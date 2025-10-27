using System;
using System.Data.Common;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BackOfTheHouse.Data.Scaffolded;

namespace BackOfTheHouse.Services;

/// <summary>
/// Minimal authentication service. Uses the application DbContext's connection
/// to query/insert into dbo.tb_users. Passwords are stored as versioned PBKDF2
/// blobs encoded as base64 in the `password_hash` nvarchar column.
/// </summary>
public class AuthService : IAuthService
{
    private readonly DockerSandwichContext? _docker;
    private readonly IDataProtector _protector;
    private readonly ILogger<AuthService> _logger;
    private readonly IEmailService _email;

    public AuthService(IServiceProvider provider, IDataProtectionProvider dp, ILogger<AuthService> logger, IEmailService email)
    {
        _docker = provider.GetService(typeof(DockerSandwichContext)) as DockerSandwichContext;
        _protector = dp.CreateProtector("BackOfTheHouse.AuthService.Mfa");
        _logger = logger;
        _email = email;
    }

    public async Task<(bool ok, string? error)> RegisterAsync(string email, string password)
    {
        email = email?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(email)) return (false, "email required");
        if (string.IsNullOrEmpty(password)) return (false, "password required");
        if (_docker == null)
        {
            _logger.LogError("Docker DB context is not available for registration");
            return (false, "server misconfigured");
        }

        try
        {
            using var conn = _docker.Database.GetDbConnection();
            await conn.OpenAsync();

            // check exists
            using (var check = conn.CreateCommand())
            {
                check.CommandText = "SELECT COUNT(1) FROM dbo.tb_users WHERE LOWER(email)=LOWER(@email)";
                var p = check.CreateParameter(); p.ParameterName = "@email"; p.Value = email; check.Parameters.Add(p);
                var exists = Convert.ToInt32(await check.ExecuteScalarAsync());
                if (exists > 0) return (false, "email already registered");
            }

            // create password blob: [version:1][salt(16)][hash(32)] stored as base64 string
            var salt = RandomNumberGenerator.GetBytes(16);
            var hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, 100_000, HashAlgorithmName.SHA256, 32);
            var blob = new byte[1 + salt.Length + hash.Length];
            blob[0] = 0x01;
            Buffer.BlockCopy(salt, 0, blob, 1, salt.Length);
            Buffer.BlockCopy(hash, 0, blob, 1 + salt.Length, hash.Length);
            var blobb64 = Convert.ToBase64String(blob);

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO dbo.tb_users (email, password_hash, created_at, is_admin) VALUES (@email, @ph, SYSUTCDATETIME(), 0)";
                var pe = cmd.CreateParameter(); pe.ParameterName = "@email"; pe.Value = email; cmd.Parameters.Add(pe);
                var pp = cmd.CreateParameter(); pp.ParameterName = "@ph"; pp.Value = blobb64; cmd.Parameters.Add(pp);
                await cmd.ExecuteNonQueryAsync();
            }

            // Try to send a welcome email. Failure to send should NOT fail registration.
            try
            {
                await _email.SendWelcomeAsync(email);
            }
            catch (System.Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send welcome email to {email}", email);
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Register failed");
            return (false, "server error");
        }
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        email = email?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(email)) return false;
        if (_docker == null)
        {
            _logger.LogWarning("Docker DB context is not available when checking email existence");
            return false;
        }

        try
        {
            using var conn = _docker.Database.GetDbConnection();
            await conn.OpenAsync();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(1) FROM dbo.tb_users WHERE LOWER(email)=LOWER(@email)";
            var p = cmd.CreateParameter(); p.ParameterName = "@email"; p.Value = email; cmd.Parameters.Add(p);
            var exists = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return exists > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "EmailExistsAsync failed");
            return false;
        }
    }

    public async Task<(bool ok, int status, string? token, bool requiresMfa, string? mfaToken, string? error)> AuthenticateAsync(string email, string password)
    {
        email = email?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
            return (false, 401, null, false, null, "invalid credentials");

        if (_docker == null)
        {
            _logger.LogError("Docker DB context is not available for authentication");
            return (false, 500, null, false, null, "server misconfigured");
        }

        try
        {
            using var conn = _docker.Database.GetDbConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, email, password_hash, is_admin, created_at, mfa_secret FROM dbo.tb_users WHERE LOWER(email)=LOWER(@email)";
            var p = cmd.CreateParameter(); p.ParameterName = "@email"; p.Value = email; cmd.Parameters.Add(p);

            using var rdr = await cmd.ExecuteReaderAsync();
            if (!await rdr.ReadAsync())
            {
                // don't reveal whether user exists
                return (false, 401, null, false, null, "Invalid credentials");
            }

            var stored = rdr.IsDBNull(2) ? null : rdr.GetString(2);
            var mfaSecretProtected = rdr.IsDBNull(5) ? null : rdr.GetString(5);

            if (string.IsNullOrEmpty(stored))
            {
                return (false, 401, null, false, null, "Invalid credentials");
            }

            byte[] blob;
            try { blob = Convert.FromBase64String(stored); }
            catch { return (false, 401, null, false, null, "Invalid credentials"); }

            if (blob.Length < 1 + 16 + 32) return (false, 401, null, false, null, "Invalid credentials");
            if (blob[0] != 0x01) return (false, 401, null, false, null, "Invalid credentials");
            var salt = new byte[16]; Buffer.BlockCopy(blob, 1, salt, 0, 16);
            var hash = new byte[32]; Buffer.BlockCopy(blob, 1 + 16, hash, 0, 32);

            var inputHash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, 100_000, HashAlgorithmName.SHA256, 32);
            if (!CryptographicOperations.FixedTimeEquals(hash, inputHash))
            {
                return (false, 401, null, false, null, "Invalid credentials");
            }

            // if mfa_secret present, require MFA flow
            if (!string.IsNullOrEmpty(mfaSecretProtected))
            {
                // Return a temporary mfaToken (simple GUID) that frontend will use to call /mfa/verify
                var mfaToken = Guid.NewGuid().ToString("D");
                // For simplicity we don't persist the mfaToken server-side in this minimal implementation.
                return (true, 200, null, true, mfaToken, null);
            }

            // Generate a simple token (not a real JWT) — include user id so server-side APIs can map requests to the user
            var userId = rdr.IsDBNull(0) ? 0 : rdr.GetInt32(0);
            // payload: guid:userId:email:ticks
            var tokenPayload = $"{Guid.NewGuid():N}:{userId}:{email}:{DateTime.UtcNow.Ticks}";
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(tokenPayload));

            return (true, 200, token, false, null, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authenticate failed");
            return (false, 500, null, false, null, "server error");
        }
    }
}
