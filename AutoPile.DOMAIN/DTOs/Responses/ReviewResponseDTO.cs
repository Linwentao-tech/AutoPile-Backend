using AutoPile.DOMAIN.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DOMAIN.DTOs.Responses
{
    public class ReviewResponseDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }
        public byte[] Image { get; set; }
        public string ImageContentType { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserResponseDTO User { get; set; }
        public ProductResponseDTO Product { get; set; }

        public string? Base64Image => Image != null && ImageContentType != null
            ? $"data:{ImageContentType};base64,{Convert.ToBase64String(Image)}"
            : null;
    }
}