using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace PracticaAPI.Middleware;

public class IpFilterMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IpFilterMiddleware> _logger;
    private readonly string _allowedIp;

    public IpFilterMiddleware(RequestDelegate next, ILogger<IpFilterMiddleware> logger, IConfiguration config)
    {
        _next = next;
        _logger = logger;
        _allowedIp = config["AllowedIP"] ?? "187.155.101.200"; // Default si no está en config
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = GetClientIpAddress(context);
        var path = context.Request.Path.Value?.ToLower();
        _logger.LogInformation($"IP Filter - Path: {path}, Client IP: {clientIp}, Allowed IP: {_allowedIp}");

        // NO hay excepciones: todo pasa por el filtro
        if (clientIp != _allowedIp)
        {
            _logger.LogWarning($"IP Filter - BLOCKED: Client IP '{clientIp}' is not authorized. Allowed: '{_allowedIp}'");
            context.Response.StatusCode = 403; // Forbidden
            context.Response.ContentType = "application/json";
            var errorResponse = new
            {
                Error = "Access Denied",
                Message = "Your IP address is not authorized to access this API.",
                ClientIP = clientIp,
                AllowedIP = _allowedIp,
                Path = path,
                Timestamp = DateTime.UtcNow
            };
            await context.Response.WriteAsJsonAsync(errorResponse);
            return;
        }

        await _next(context);
    }

    private string GetClientIpAddress(HttpContext context)
    {
        // Siempre usar la primera IP de X-Forwarded-For si existe
        var forwardedHeader = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedHeader))
        {
            var ips = forwardedHeader.Split(',');
            return ips[0].Trim();
        }
        // Fallback: X-Real-IP
        var realIpHeader = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIpHeader))
        {
            return realIpHeader.Trim();
        }
        // Fallback: RemoteIpAddress (solo para desarrollo local)
        var remoteIp = context.Connection.RemoteIpAddress?.ToString();
        if (remoteIp != null && remoteIp.Contains("::ffff:"))
        {
            remoteIp = remoteIp.Replace("::ffff:", "");
        }
        return remoteIp ?? "Unknown";
    }
} 