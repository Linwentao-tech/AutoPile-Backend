using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DOMAIN.Models.Entities
{
    public class ApplicationUser : IdentityUser<string>
    {
        public ApplicationUser()
        {
            Orders = new List<Orders>();
            Reviews = new List<Reviews>();
            ShoppingCartItems = new List<ShoppingCartItems>();
        }

        public string? EmailVerifyToken { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public ICollection<Orders> Orders { get; set; }
        public ICollection<Reviews> Reviews { get; set; }
        public ICollection<ShoppingCartItems> ShoppingCartItems { get; set; }
    }
}