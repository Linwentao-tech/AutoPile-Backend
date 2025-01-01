using AutoPile.DOMAIN.Enum;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DOMAIN.Models.Entities
{
    public class Product
    {
        public Product()
        {
            ProductMedias = new List<ProductMedia>();
        }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string ProductInfo { get; set; }
        public string SKU { get; set; }
        public decimal Price { get; set; }
        public decimal ComparePrice { get; set; }
        public bool IsInStock { get; set; }
        public int StockQuantity { get; set; }
        public Ribbon Ribbon { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<ProductMedia> ProductMedias { get; set; }

        //public ICollection<OrderItem> OrderItems { get; set; }
        //public ICollection<Review> Reviews { get; set; }
        //public ICollection<ShoppingCartItem> ShoppingCartItems { get; set; }
        public Category Category { get; set; }
    }
}