using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DOMAIN.Models.Entities
{
    public class ShoppingCartItems
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ApplicationUser User { get; set; }
        public Products Products { get; set; }
    }
}