using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DOMAIN.Models.Entities
{
    public class ShoppingCartItem
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public ApplicationUser User { get; set; }
    }
}