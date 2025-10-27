using System.Threading.Tasks;

namespace BackOfTheHouse.Services;

public interface IAuthService
{
    Task<(bool ok, string? error)> RegisterAsync(string email, string password);
    Task<bool> EmailExistsAsync(string email);
    Task<(bool ok, int status, string? token, bool requiresMfa, string? mfaToken, string? error)> AuthenticateAsync(string email, string password);
}
