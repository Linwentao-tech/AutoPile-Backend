using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DOMAIN.DTOs.Responses
{
    public class UserResponseDTO
    {
        public UserResponseDTO()
        {
            Roles = new List<string>();
        }

        public string UserName { get; set; }
        public string Email { get; set; }

        public string Id { get; set; }
        public IList<string> Roles { get; set; }
        public bool EmailConfirmed { get; set; }
    }
}