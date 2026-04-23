namespace TheNextLevel.Application.Interfaces;

public interface IAuthService
{
    Task<bool> LoginAsync(string username, string password);
    Task<string?> GetTokenAsync();
    Task LogoutAsync();
}
