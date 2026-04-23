using System.Net.Http.Headers;
using System.Net.Http.Json;
using TheNextLevel.Application.Interfaces;
using TheNextLevel.Infrastructure.Data;

namespace TheNextLevel.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly TursoClient _tursoClient;
    private const string TokenKey = "auth_token";

    public AuthService(HttpClient httpClient, TursoClient tursoClient)
    {
        _httpClient = httpClient;
        _tursoClient = tursoClient;
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

            var tursoToken = await FetchTursoTokenAsync(result.Token);
            if (tursoToken is null)
                return false;

            _tursoClient.UpdateAuthToken(tursoToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<string?> FetchTursoTokenAsync(string jwtToken)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "auth/turso-token");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return null;

            var result = await response.Content.ReadFromJsonAsync<TursoTokenResponse>();
            return result?.AuthToken;
        }
        catch
        {
            return null;
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
    private record TursoTokenResponse(string AuthToken);
}
