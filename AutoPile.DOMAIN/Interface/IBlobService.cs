using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DOMAIN.Interface
{
    public interface IBlobService
    {
        Task<string> UploadImageAsync(IFormFile file);

        Task DeleteImageAsync(string imageUrl);

        Task<string> UpdateImageAsync(string oldImageUrl, IFormFile newFile);
    }
}