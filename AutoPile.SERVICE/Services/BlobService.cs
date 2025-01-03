using AutoPile.DATA.Exceptions;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.SERVICE.Services
{
    public interface IBlobService
    {
        Task<string> UploadImageAsync(IFormFile file);

        Task DeleteImageAsync(string imageUrl);

        Task<string> UpdateImageAsync(string oldImageUrl, IFormFile newFile);
    }

    public class BlobService : IBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName = "reviews";

        public BlobService(IConfiguration configuration)
        {
            _blobServiceClient = new BlobServiceClient(Environment.GetEnvironmentVariable("BlobStorage"));
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync();

            // Create unique blob name
            string blobName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var blobClient = containerClient.GetBlobClient(blobName);

            // Upload the file
            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });

            // Generate SAS token
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _containerName,
                BlobName = blobName,
                Resource = "b", // b for blob
                StartsOn = DateTimeOffset.UtcNow,
                ExpiresOn = DateTimeOffset.UtcNow.AddYears(1), // Or your preferred expiration
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read); // Only allow read

            var sasToken = blobClient.GenerateSasUri(sasBuilder).ToString();
            return sasToken; // This will return the full URL with SAS token
        }

        public async Task DeleteImageAsync(string imageUrl)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var uri = new Uri(imageUrl);
            string blobName = Path.GetFileName(uri.LocalPath);
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
        }

        public async Task<string> UpdateImageAsync(string oldImageUrl, IFormFile newFile)
        {
            if (newFile == null)
            {
                throw new BadRequestException("Empty file");
            }

            if (!string.IsNullOrEmpty(oldImageUrl))
            {
                await DeleteImageAsync(oldImageUrl);
            }

            return await UploadImageAsync(newFile);
        }
    }
}