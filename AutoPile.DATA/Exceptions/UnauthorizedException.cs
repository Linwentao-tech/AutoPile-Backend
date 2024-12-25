using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DATA.Exceptions
{
    public class UnauthorizedException : AbstractHTTPexception
    {
        public UnauthorizedException() : base(401, "Unauthorized ")
        {
        }

        public UnauthorizedException(string message) : base(400, message)
        {
        }
    }
}