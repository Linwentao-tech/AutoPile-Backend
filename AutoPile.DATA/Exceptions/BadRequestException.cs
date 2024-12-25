using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DATA.Exceptions
{
    public class BadRequestException : AbstractHTTPexception
    {
        public BadRequestException() : base(400, "Bad Request")
        {
        }

        public BadRequestException(string message) : base(400, message)
        {
        }
    }
}