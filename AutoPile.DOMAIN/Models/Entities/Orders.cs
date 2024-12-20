﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DOMAIN.Models.Entities
{
    public class Orders
    {
        public Orders()
        {
            OrderItems = new List<OrderItems>();
        }

        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; }
        public string PaymentMethod { get; set; }
        public string StripeSessionId { get; set; }
        public string ShippingAddress_Line1 { get; set; }
        public string ShippingAddress_Line2 { get; set; }
        public string ShippingAddress_City { get; set; }
        public string ShippingAddress_Country { get; set; }
        public string ShippingAddress_State { get; set; }
        public string ShippingAddress_PostalCode { get; set; }

        public ApplicationUser User { get; set; }

        public ICollection<OrderItems> OrderItems { get; set; }
    }
}