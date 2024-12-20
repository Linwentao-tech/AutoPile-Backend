using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DOMAIN.Models.Entities
{
    public class OrderItems
    {
        public int Id { get; set; }
        public Guid OrderId { get; set; }

        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public Orders Orders { get; set; }
        public Products Products { get; set; }
    }
}