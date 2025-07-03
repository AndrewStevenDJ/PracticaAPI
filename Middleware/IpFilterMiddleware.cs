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
            context.Response.ContentType = "application/json";
            
            var errorResponse = new
            {
                Error = "Access Denied",
                Message = "Your IP address is not authorized to access this API.",
                ClientIP = clientIp,
                AllowedIP = _allowedIp,
                Timestamp = DateTime.UtcNow
            };
            
            await context.Response.WriteAsJsonAsync(errorResponse);
            return;
        }

        await _next(context);
    }

    private string GetClientIpAddress(HttpContext context)
    {
        // Obtener la IP real del cliente, considerando proxies y load balancers
        var forwardedHeader = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedHeader))
        {
            // Tomar la primera IP de la lista (IP original del cliente)
            var ips = forwardedHeader.Split(',');
            return ips[0].Trim();
        }

        var realIpHeader = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIpHeader))
        {
            return realIpHeader.Trim();
        }

        var xForwardedProtoHeader = context.Request.Headers["X-Forwarded-Proto"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xForwardedProtoHeader))
        {
            // Si hay X-Forwarded-Proto, también verificar X-Forwarded-For
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }
        }

        // Obtener la IP directa de la conexión
        var remoteIp = context.Connection.RemoteIpAddress?.ToString();
        
        // Si es IPv6, convertir a IPv4 si es posible
        if (remoteIp != null && remoteIp.Contains("::ffff:"))
        {
            remoteIp = remoteIp.Replace("::ffff:", "");
        }

        return remoteIp ?? "Unknown";
    }
} 