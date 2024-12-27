using AutoPile.DOMAIN.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DOMAIN.DTOs.Requests
{
    public class OrderItemCreateDTO
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public int Quantity { get; set; }
    }
}