using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DOMAIN.DTOs.Requests
{
    public class ShoppingCartItemRequestDto
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateShoppingCartItemDto
    {
        public int Quantity { get; set; }
    }
}