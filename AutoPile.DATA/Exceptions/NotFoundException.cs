using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DATA.Exceptions
{
    public class NotFoundException : AbstractHTTPexception
    {
        public NotFoundException() : base(404, "Not Found")
        {
        }

        public NotFoundException(string message) : base(404, message)
        {
        }
    }
}