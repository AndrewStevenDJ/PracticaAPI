using System.Net;

namespace PracticaAPI.Middleware;

public class IpFilterMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IpFilterMiddleware> _logger;
    private readonly string _allowedIp = "187.155.101.200";

    public IpFilterMiddleware(RequestDelegate next, ILogger<IpFilterMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = GetClientIpAddress(context);
        
        _logger.LogInformation($"Request from IP: {clientIp}");

        // Verificar si la IP está permitida
        if (clientIp != _allowedIp)
        {
            _logger.LogWarning($"Access denied for IP: {clientIp}");
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            await context.Response.WriteAsync("Access denied. Your IP is not authorized.");
            return;
        }

        _logger.LogInformation($"Access granted for IP: {clientIp}");
        await _next(context);
    }

    private string GetClientIpAddress(HttpContext context)
    {
        // Obtener la IP real del cliente, considerando proxies
        var forwardedHeader = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedHeader))
        {
            // X-Forwarded-For puede contener múltiples IPs, tomar la primera
            return forwardedHeader.Split(',')[0].Trim();
        }

        var realIpHeader = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIpHeader))
        {
            return realIpHeader;
        }

        // Si no hay headers de proxy, usar la IP de conexión
        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}

// Clase de extensión para facilitar el registro del middleware
public static class IpFilterMiddlewareExtensions
{
    public static IApplicationBuilder UseIpFilter(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<IpFilterMiddleware>();
    }
} 