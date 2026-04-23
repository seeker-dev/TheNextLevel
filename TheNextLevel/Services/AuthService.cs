using System.Net.Http.Json;
using TheNextLevel.Application.Interfaces;

namespace TheNextLevel.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private const string TokenKey = "auth_token";

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("auth/login", new { username, password });
            if (!response.IsSuccessStatusCode)
                return false;

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (result?.Token is null)
                return false;

            await SecureStorage.Default.SetAsync(TokenKey, result.Token);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public Task<string?> GetTokenAsync() =>
        SecureStorage.Default.GetAsync(TokenKey);

    public Task LogoutAsync()
    {
        SecureStorage.Default.Remove(TokenKey);
        return Task.CompletedTask;
    }

    private record LoginResponse(string Token, DateTime ExpiresAt);
}
