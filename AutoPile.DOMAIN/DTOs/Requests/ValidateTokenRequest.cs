using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DOMAIN.DTOs.Requests
{
    public class ValidateTokenRequest
    {
        public string Email { get; set; }
        public string Token { get; set; }
    }
}