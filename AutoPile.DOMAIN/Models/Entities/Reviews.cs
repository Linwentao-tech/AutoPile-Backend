using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DOMAIN.Models.Entities
{
    public class Reviews
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public Guid ProductId { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public ApplicationUser User { get; set; }
        public Products Products { get; set; }
    }
}