﻿using AutoPile.DOMAIN.Enum;
using AutoPile.DOMAIN.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DOMAIN.DTOs.Responses
{
    public class ProductResponseDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string productDescription { get; set; }
        public string ProductInfo { get; set; }
        public string SKU { get; set; }
        public decimal Price { get; set; }
        public decimal ComparePrice { get; set; }
        public bool IsInStock { get; set; }
        public int StockQuantity { get; set; }
        public Ribbon Ribbon { get; set; }
        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
        public Category Category { get; set; }
        public List<ProductMediaResponseDTO> ProductMedias { get; set; }
    }
}