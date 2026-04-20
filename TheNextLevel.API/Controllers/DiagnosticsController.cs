using Microsoft.AspNetCore.Mvc;

namespace TheNextLevel.API.Controllers;

[ApiController]
[Route("[controller]")]
public class DiagnosticsController : ControllerBase
{
    [HttpGet("alive")]
    public string IsAlive()
    {
        return "I am alive.";
    }
}