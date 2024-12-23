using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DOMAIN.Models.Entities
{
    public class ProductMedia
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Url { get; set; }
        public string FullUrl { get; set; }
        public string MediaType { get; set; }
        public string AltText { get; set; }
        public string Title { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Product Product { get; set; }
    }
}