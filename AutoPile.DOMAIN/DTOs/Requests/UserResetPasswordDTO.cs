using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DOMAIN.DTOs.Requests
{
    public class UserResetPasswordDTO
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
        public string EmailVerifyToken { get; set; }
    }
}