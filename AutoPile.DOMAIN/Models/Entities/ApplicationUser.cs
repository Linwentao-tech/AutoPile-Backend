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
            Orders = new List<Order>();
            Reviews = new List<Review>();
            ShoppingCartItems = new List<ShoppingCartItem>();
        }

        public string? EmailVerifyToken { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public ICollection<Order> Orders { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<ShoppingCartItem> ShoppingCartItems { get; set; }
    }
}