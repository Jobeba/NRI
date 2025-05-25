using Microsoft.AspNetCore.Http; 
using Microsoft.AspNetCore.Http.Extensions; 
using System.Threading.Tasks;
using System.Collections.Specialized; 
using System.Linq;
using System;

namespace NRI.Services
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JwtService _jwtService;

        public AuthMiddleware(RequestDelegate next, JwtService jwtService)
        {
            _next = next;
            _jwtService = jwtService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.Value?.StartsWith("/api") == true &&
                !context.Request.Path.Value.StartsWith("/api/auth"))
            {
                if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader) ||
                    string.IsNullOrEmpty(authHeader) ||
                    !_jwtService.TryValidateToken(authHeader.ToString().Replace("Bearer ", ""), out _))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized");
                    return;
                }
            }

            await _next(context);
        }
    }
}
