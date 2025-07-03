using Microsoft.AspNetCore.Http;

namespace PracticaAPI.Middleware;

public class IpFilterMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _allowedIp = "187.155.101.200";

    public IpFilterMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = GetClientIpAddress(context);
        
        // Permitir acceso a Swagger y endpoints de autenticación
        var path = context.Request.Path.Value?.ToLower();
        if (path != null && (path.StartsWith("/swagger") || path.StartsWith("/api/auth")))
        {
            await _next(context);
            return;
        }

        // Verificar si la IP del cliente está permitida
        if (clientIp != _allowedIp)
        {
            context.Response.StatusCode = 403; // Forbidden
            await context.Response.WriteAsync("Access denied. Your IP is not authorized.");
            return;
        }

        await _next(context);
    }

    private string GetClientIpAddress(HttpContext context)
    {
        // Obtener la IP real del cliente, considerando proxies
        var forwardedHeader = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedHeader))
        {
            return forwardedHeader.Split(',')[0].Trim();
        }

        var realIpHeader = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIpHeader))
        {
            return realIpHeader;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
} 