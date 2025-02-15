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

        [BsonElement("name")]
        [BsonRequired]
        public string Name { get; set; }

        [BsonElement("productDescription")]
        [BsonRequired]
        public string productDescription { get; set; }

        [BsonElement("productInfo")]
        [BsonRequired]
        public string ProductInfo { get; set; }

        [BsonElement("sku")]
        [BsonRequired]
        public string SKU { get; set; }

        [BsonElement("price")]
        [BsonRequired]
        public decimal Price { get; set; }

        [BsonElement("comparePrice")]
        public decimal? ComparePrice { get; set; }

        [BsonElement("isInStock")]
        [BsonRequired]
        public bool IsInStock { get; set; }

        [BsonElement("stockQuantity")]
        [BsonRequired]
        public int StockQuantity { get; set; }

        [BsonElement("ribbon")]
        [BsonRequired]
        public Ribbon Ribbon { get; set; }

        [BsonElement("createdAt")]
        [BsonRequired]
        public DateTime CreatedAt { get; set; }

        [BsonElement("updatedAt")]
        [BsonRequired]
        public DateTime UpdatedAt { get; set; }

        [BsonElement("productMedias")]
        [BsonRequired]
        public List<ProductMedia> ProductMedias { get; set; }

        [BsonElement("category")]
        [BsonRequired]
        public Category Category { get; set; }
    }
}