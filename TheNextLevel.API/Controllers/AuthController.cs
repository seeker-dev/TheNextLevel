using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace TheNextLevel.API.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config)
    {
        _config = config;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var expectedUsername = _config["OPUS_USERNAME"];
        var passwordHash = _config["OPUS_PASSWORD_HASH"];
        var signingKey = _config["JWT_SIGNING_KEY"];

        if (string.IsNullOrEmpty(expectedUsername) || string.IsNullOrEmpty(passwordHash) || string.IsNullOrEmpty(signingKey))
            return StatusCode(500, "Authentication is not configured.");

        if (!string.Equals(request.Username, expectedUsername, StringComparison.Ordinal))
            return Unauthorized();

        if (!BCrypt.Net.BCrypt.Verify(request.Password, passwordHash))
            return Unauthorized();

        var expires = DateTime.UtcNow.AddHours(1);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, request.Username),
                new Claim("accountId", "1"),
                new Claim(JwtRegisteredClaimNames.Iat,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64)
            ],
            expires: expires,
            signingCredentials: credentials
        );

        return Ok(new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token), expires));
    }
}

public record LoginRequest(string Username, string Password);
public record LoginResponse(string Token, DateTime ExpiresAt);
