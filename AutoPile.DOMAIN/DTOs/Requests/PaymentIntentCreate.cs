using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DOMAIN.DTOs.Requests
{
    public class PaymentIntentCreate
    {
        public Item[] Items { get; set; }
    }

    public class Item
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
    }
}