using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DATA.Exceptions
{
    public class ForbiddenException : AbstractHTTPexception
    {
        public ForbiddenException() : base(403, "Forbidden")
        {
        }

        public ForbiddenException(string message) : base(403, message)
        {
        }
    }
}