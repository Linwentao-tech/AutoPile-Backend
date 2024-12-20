using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DOMAIN.Models.Entities
{
    public class Products
    {
        public Products()
        {
            ProductMedias = new List<ProductMedia>();
            OrderItems = new List<OrderItems>();
            Reviews = new List<Reviews>();
            ShoppingCartItems = new List<ShoppingCartItems>();
        }

        public Guid Id { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ProductInfo { get; set; }
        public string SKU { get; set; }
        public decimal Price { get; set; }
        public decimal ComparePrice { get; set; }
        public bool IsInStock { get; set; }
        public int StockQuantity { get; set; }
        public string Ribbon { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<ProductMedia> ProductMedias { get; set; }
        public ICollection<OrderItems> OrderItems { get; set; }
        public ICollection<Reviews> Reviews { get; set; }
        public ICollection<ShoppingCartItems> ShoppingCartItems { get; set; }
        public Categories Categories { get; set; }
    }
}