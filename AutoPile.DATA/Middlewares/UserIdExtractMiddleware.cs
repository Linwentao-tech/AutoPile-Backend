using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DATA.Middlewares
{
    public class UserIdExtractMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserIdExtractMiddleware> _logger;

        public UserIdExtractMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<UserIdExtractMiddleware> logger)
        {
            _next = next;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpcontext)
        {
            try
            {
                var token = httpcontext.Request.Cookies["AuthToken"];
                if (token != null)
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);

                    ValidateToken(token);

                    var userId = jwtToken.Claims.FirstOrDefault(c =>
                           c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

                    if (userId != null)
                    {
                        httpcontext.Items["UserId"] = userId;
                        _logger.LogInformation("user id is set");
                    }
                }
            }
            catch (SecurityTokenException ex)
            {
                httpcontext.Response.StatusCode = 401;
                await httpcontext.Response.WriteAsync($"Invalid token: {ex.Message}");
                return;
            }
            catch (Exception ex)
            {
                httpcontext.Response.StatusCode = 500;
                await httpcontext.Response.WriteAsync($"An error occurred while processing the token: {ex.Message}");
                return;
            }

            await _next(httpcontext);
        }

        private void ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("JWTKEY") ?? _configuration["Jwt:Key"]);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = Environment.GetEnvironmentVariable("ISSUER") ?? _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = Environment.GetEnvironmentVariable("AUDIENCE") ?? _configuration["Jwt:Audience"],
                ClockSkew = TimeSpan.Zero,
                ValidateLifetime = true
            }, out SecurityToken validatedToken);
        }
    }
}