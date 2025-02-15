using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DOMAIN.DTOs.Responses
{
    public class UserInfoResponseDTO
    {
        public UserInfoResponseDTO()
        {
            Roles = new List<string>();
        }

        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public IList<string> Roles { get; set; }
        public bool EmailConfirmed { get; set; }
    }
}