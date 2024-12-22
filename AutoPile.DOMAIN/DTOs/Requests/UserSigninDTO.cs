using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DOMAIN.DTOs.Requests
{
    public class UserSigninDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}