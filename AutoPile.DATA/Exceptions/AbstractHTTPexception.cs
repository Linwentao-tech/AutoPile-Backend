using AutoPile.DOMAIN.Interface;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace AutoPile.DATA.Exceptions
{
    public abstract class AbstractHTTPexception : Exception, IExceptionHandler
    {
        public int StatusCode { get; set; }
        public Guid Id { get; } = Guid.NewGuid();
        public DateTime DateTime { get; } = DateTime.Now;

        protected AbstractHTTPexception(int statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }

        public Task HandleExceptionAsync(HttpContext context)
        {
            context.Response.StatusCode = StatusCode;
            context.Response.ContentType = "application/json";

            var response = new
            {
                Success = false,
                base.Message,
                StatusCode,
            };
            return context.Response.WriteAsync(JsonConvert.SerializeObject(response));
        }
    }
}