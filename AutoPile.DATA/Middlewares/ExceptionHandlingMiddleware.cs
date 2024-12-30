using AutoPile.DATA.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DATA.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (AbstractHTTPexception ex)
            {
                _logger.LogError(ex, $"Http status: {ex.StatusCode}, Error Id: {ex.Id}, Error Time: {ex.DateTime}");
                await ex.HandleExceptionAsync(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Http status: {500}, Error Id: {Guid.NewGuid()}, Error Time: {DateTime.Now}");
                await HandleGeneralExceptionAsync(httpContext, ex);
            }
        }

        private Task HandleGeneralExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var response = new
            {
                Code = (int)HttpStatusCode.InternalServerError,
                Message = "Internal Server Error",
                Success = false,
                Detailed = ex.Message
            };
            return context.Response.WriteAsync(JsonConvert.SerializeObject(response));
        }
    }
}