using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DOMAIN.DTOs.Requests
{
    public class ProductMediaUpdateDto
    {
        public string Url { get; set; }
        public string FullUrl { get; set; }
        public string MediaType { get; set; }
        public string AltText { get; set; }
        public string Title { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}