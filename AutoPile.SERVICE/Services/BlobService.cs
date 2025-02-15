using AutoPile.DATA.Exceptions;
using AutoPile.DOMAIN.Interface;
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
    public class BlobService : IBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName = "reviews";

        public BlobService(IConfiguration configuration)
        {
            string connectionString = Environment.GetEnvironmentVariable("BlobStorage") ?? configuration["Azure:BlobStorage:ConnectionString"]
        ?? throw new InvalidOperationException("Blob storage connection string not configured");
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync();

          
            string blobName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var blobClient = containerClient.GetBlobClient(blobName);

            
            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });

           
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _containerName,
                BlobName = blobName,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow,
                ExpiresOn = DateTimeOffset.UtcNow.AddYears(1),
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasToken = blobClient.GenerateSasUri(sasBuilder).ToString();
            return sasToken;
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
