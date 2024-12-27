using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DOMAIN.DTOs.Requests
{
    public class ReviewCreateDTO
    {
        public string UserId { get; set; }
        public string ProductId { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }
        public IFormFile Image { get; set; }
    }
}