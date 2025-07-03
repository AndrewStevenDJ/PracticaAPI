using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace PracticaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Permitir acceso sin autenticación para pruebas
public class TestController : ControllerBase
{
    [HttpGet("ip")]
    public IActionResult GetClientIp()
    {
        var clientIp = GetClientIpAddress();
        
        return Ok(new
        {
            ClientIP = clientIp,
            IsAllowed = clientIp == "187.155.101.200",
            Message = clientIp == "187.155.101.200" 
                ? "Your IP is authorized" 
                : "Your IP is not authorized"
        });
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new
        {
            Status = "API is running",
            AllowedIP = "187.155.101.200",
            Timestamp = DateTime.UtcNow
        });
    }

    [HttpGet("auth-test")]
    [Authorize] // Este endpoint requiere autenticación
    public IActionResult TestAuth()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        
        return Ok(new
        {
            Message = "Authentication successful!",
            UserId = userId,
            Username = username,
            Timestamp = DateTime.UtcNow
        });
    }

    private string GetClientIpAddress()
    {
        // Obtener la IP real del cliente, considerando proxies
        var forwardedHeader = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedHeader))
        {
            return forwardedHeader.Split(',')[0].Trim();
        }

        var realIpHeader = Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIpHeader))
        {
            return realIpHeader;
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
} 