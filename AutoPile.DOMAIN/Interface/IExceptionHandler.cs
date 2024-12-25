using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DOMAIN.Interface
{
    public interface IExceptionHandler
    {
        public Task HandleExceptionAsync(HttpContext context);
    }
}