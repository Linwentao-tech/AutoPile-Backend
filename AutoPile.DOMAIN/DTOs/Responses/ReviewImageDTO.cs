using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DOMAIN.DTOs.Responses
{
    public class ReviewImageDTO
    {
        public byte[] Image { get; init; }
        public string ImageContentType { get; init; }

        public string Base64Image => Image != null && ImageContentType != null
            ? $"data:{ImageContentType};base64,{Convert.ToBase64String(Image)}"
            : null;
    }
}