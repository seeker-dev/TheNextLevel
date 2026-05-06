namespace TheNextLevel.Application.Interfaces;

public interface IAuthService
{
    Task<bool> InitializeAsync(CancellationToken ct = default);
    Task<bool> LoginAsync(string username, string password, CancellationToken ct = default);
    Task<string?> GetTokenAsync(CancellationToken ct = default);
    Task LogoutAsync(CancellationToken ct = default);
}
