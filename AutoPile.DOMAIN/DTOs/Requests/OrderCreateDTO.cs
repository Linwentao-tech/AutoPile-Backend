﻿using AutoPile.DOMAIN.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DOMAIN.DTOs.Requests
{
    public class OrderCreateDTO
    {
        public string UserId { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string ShippingAddress_Line1 { get; set; }
        public string ShippingAddress_Line2 { get; set; }
        public string ShippingAddress_City { get; set; }
        public string ShippingAddress_Country { get; set; }
        public string ShippingAddress_State { get; set; }
        public string ShippingAddress_PostalCode { get; set; }
    }
}