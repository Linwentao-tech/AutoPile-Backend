﻿using AutoMapper;
using AutoPile.DATA.Cache;
using AutoPile.DATA.Data;
using AutoPile.DATA.Exceptions;
using AutoPile.DOMAIN.DTOs.Requests;
using AutoPile.DOMAIN.DTOs.Responses;
using AutoPile.DOMAIN.Models.Entities;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.SERVICE.Services
{
    public class ProductService : IProductService
    {
        private readonly AutoPileMongoDbContext _autoPileMongoDbContext;
        private readonly IMapper _mapper;
        private readonly IProductCache _productCache;

        public ProductService(AutoPileMongoDbContext autoPileMongoDbContext, IMapper mapper, IProductCache productCache)
        {
            _autoPileMongoDbContext = autoPileMongoDbContext;
            _mapper = mapper;
            _productCache = productCache;
        }

        public async Task<ProductResponseDTO> CreateProductAsync(ProductCreateDTO productCreateDTO)
        {
            var existingProduct = _autoPileMongoDbContext.Products.FirstOrDefault(p => p.SKU == productCreateDTO.SKU);
            if (existingProduct != null)
            {
                throw new BadRequestException($"Product with SKU {productCreateDTO.SKU} already exists");
            }
            var product = _mapper.Map<Product>(productCreateDTO);
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            await _autoPileMongoDbContext.Products.AddAsync(product);
            await _autoPileMongoDbContext.SaveChangesAsync();
            return _mapper.Map<ProductResponseDTO>(product);
        }

        public async Task<ProductResponseDTO> GetProductByIdAsync(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId productObjectId))
            {
                throw new BadRequestException("Invalid product ID format");
            }

            var cachedProduct = await _productCache.GetProductAsync(id);
            if (cachedProduct != null)
            {
                return cachedProduct;
            }

            var product = await _autoPileMongoDbContext.Products.FindAsync(productObjectId) ?? throw new NotFoundException($"Product with Id {id} not found");
            var productResponseDTO = _mapper.Map<ProductResponseDTO>(product);
            await _productCache.SetProductAsync(id, productResponseDTO);
            return productResponseDTO;
        }

        public async Task<IEnumerable<ProductResponseDTO>> GetProductsListAsync()
        {
            var products = await _autoPileMongoDbContext.Products.OrderBy(p => p.Name).ToListAsync();
            if (products.Count == 0)
            {
                throw new NotFoundException("No products found");
            }

            return _mapper.Map<IEnumerable<ProductResponseDTO>>(products);
        }

        public async Task DeleteProductByIdAsync(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId productObjectId))
            {
                throw new BadRequestException("Invalid product ID format");
            }
            var product = await _autoPileMongoDbContext.Products.FindAsync(productObjectId) ?? throw new NotFoundException($"Product with Id {id} not found");
            _autoPileMongoDbContext.Remove(product);
            await _autoPileMongoDbContext.SaveChangesAsync();

            await _productCache.DeleteProductAsync(id);
        }

        public async Task<ProductResponseDTO> UpdateProductByIdAsync(ProductUpdateDTO productUpdateDTO, string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId productObjectId))
            {
                throw new BadRequestException("Invalid product ID format");
            }

            var product = await _autoPileMongoDbContext.Products.FindAsync(productObjectId)
                ?? throw new NotFoundException($"Product with Id {id} not found");

            if (productUpdateDTO.Name != null)
                product.Name = productUpdateDTO.Name;
            if (productUpdateDTO.Description != null)
                product.productDescription = productUpdateDTO.Description;
            if (productUpdateDTO.ProductInfo != null)
                product.ProductInfo = productUpdateDTO.ProductInfo;
            if (productUpdateDTO.SKU != null)
                product.SKU = productUpdateDTO.SKU;
            if (productUpdateDTO.Price.HasValue)
                product.Price = productUpdateDTO.Price.Value;
            if (productUpdateDTO.ComparePrice.HasValue)
                product.ComparePrice = productUpdateDTO.ComparePrice.Value;
            if (productUpdateDTO.IsInStock.HasValue)
                product.IsInStock = productUpdateDTO.IsInStock.Value;
            if (productUpdateDTO.StockQuantity.HasValue)
                product.StockQuantity = productUpdateDTO.StockQuantity.Value;
            if (productUpdateDTO.Ribbon.HasValue)
                product.Ribbon = productUpdateDTO.Ribbon.Value;
            if (productUpdateDTO.Category.HasValue)
                product.Category = productUpdateDTO.Category.Value;
            if (productUpdateDTO.ProductMedias != null)
                product.ProductMedias = _mapper.Map<List<ProductMedia>>(productUpdateDTO.ProductMedias);

            product.UpdatedAt = DateTime.UtcNow;

            _autoPileMongoDbContext.Update(product);
            await _autoPileMongoDbContext.SaveChangesAsync();
            await _productCache.SetProductAsync(id, _mapper.Map<ProductResponseDTO>(product));
            return _mapper.Map<ProductResponseDTO>(product);
        }
    }
}