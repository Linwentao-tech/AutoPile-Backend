using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Diagnostics;
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
                var requestId = Activity.Current?.Id ?? httpcontext.TraceIdentifier;
                _logger.LogInformation("Request {RequestId} - Begin processing request for path: {Path}",
                    requestId, httpcontext.Request.Path);

                var token = httpcontext.Request.Cookies["AuthToken"];
                if (token != null)
                {
                    _logger.LogInformation("Request {RequestId} - Found auth token in cookies", requestId);
                    _logger.LogInformation("Token value: {TokenPrefix}...",
                        token.Substring(0, Math.Min(6, token.Length)));

                    try
                    {
                        var handler = new JwtSecurityTokenHandler();
                        var jwtToken = handler.ReadJwtToken(token);

                        // Log token details before validation
                        _logger.LogInformation("Token Issuer: {Issuer}, Expires: {Expires}",
                            jwtToken.Issuer,
                            jwtToken.ValidTo);

                        ValidateToken(token);
                        _logger.LogInformation("Token validation successful");

                        var userId = jwtToken.Claims.FirstOrDefault(c =>
                               c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
                        if (userId != null)
                        {
                            httpcontext.Items["UserId"] = userId;
                            _logger.LogInformation("Request {RequestId} - User ID {UserId} extracted and set in context",
                                requestId, userId);
                        }
                    }
                    catch (SecurityTokenExpiredException ex)
                    {
                        _logger.LogWarning("Token expired: {Message}", ex.Message);
                        throw;
                    }
                    catch (SecurityTokenInvalidIssuerException ex)
                    {
                        _logger.LogWarning("Invalid issuer: {Message}, Expected: {Expected}, Received: {Received}",
                            ex.Message,
                            Environment.GetEnvironmentVariable("ISSUER") ?? _configuration["Jwt:Issuer"],
                            ex.InvalidIssuer);
                        throw;
                    }
                    catch (SecurityTokenInvalidAudienceException ex)
                    {
                        _logger.LogWarning("Invalid audience: {Message}, Expected: {Expected}, Received: {Received}",
                            ex.Message,
                            Environment.GetEnvironmentVariable("AUDIENCE") ?? _configuration["Jwt:Audience"],
                            ex.InvalidAudience);
                        throw;
                    }
                }
                else
                {
                    _logger.LogInformation("Request {RequestId} - No auth token found in request cookies", requestId);
                }
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogError(ex, "Request {RequestId} - Security token validation failed: {Message}",
                    httpcontext.TraceIdentifier, ex.Message);
                httpcontext.Response.StatusCode = 401;
                await httpcontext.Response.WriteAsync($"Invalid token: {ex.Message}");
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Request {RequestId} - Unexpected error processing token: {Message}",
                    httpcontext.TraceIdentifier, ex.Message);
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

            // Log validation parameters
            _logger.LogInformation("Validation Parameters: Issuer: {Issuer}, Audience: {Audience}",
                Environment.GetEnvironmentVariable("ISSUER") ?? _configuration["Jwt:Issuer"],
                Environment.GetEnvironmentVariable("AUDIENCE") ?? _configuration["Jwt:Audience"]);

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