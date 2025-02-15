using AutoPile.DOMAIN.DTOs.Requests;
using AutoPile.DOMAIN.DTOs.Responses;
using AutoPile.DOMAIN.Models;
using AutoPile.SERVICE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoPile.API.Controllers
{
    /// <summary>
    /// Controller for managing product operations
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        /// <summary>
        /// Initializes a new instance of the ProductController
        /// </summary>
        /// <param name="productService">The product service for handling business logic</param>
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Creates a new product
        /// </summary>
        /// <param name="productCreateDTO">The product details to create</param>
        /// <returns>The newly created product</returns>
        /// <response code="200">Returns the newly created product</response>
        /// <response code="400">If the product data is invalid or SKU already exists</response>
        [HttpPost("CreateProduct", Name = "CreateProduct")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProductAsync([FromBody] ProductCreateDTO productCreateDTO)
        {
            var productResponseDTO = await _productService.CreateProductAsync(productCreateDTO);
            return ApiResponse<ProductResponseDTO>.OkResult(productResponseDTO);
        }

        /// <summary>
        /// Retrieves a specific product by its ID
        /// </summary>
        /// <param name="id">The ID of the product to retrieve</param>
        /// <returns>The requested product</returns>
        /// <response code="200">Returns the requested product</response>
        /// <response code="400">If the product ID format is invalid</response>
        /// <response code="404">If the product is not found</response>
        [HttpGet("{id}", Name = "GetProductById")]
        public async Task<IActionResult> GetProductById(string id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            return ApiResponse<ProductResponseDTO>.OkResult(product);
        }

        /// <summary>
        /// Deletes a specific product
        /// </summary>
        /// <param name="id">The ID of the product to delete</param>
        /// <returns>No content</returns>
        /// <response code="204">If the product was successfully deleted</response>
        /// <response code="400">If the product ID format is invalid</response>
        /// <response code="404">If the product is not found</response>
        [HttpDelete("{id}", Name = "DeleteProductById")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProductById(string id)
        {
            await _productService.DeleteProductByIdAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Updates a specific product
        /// </summary>
        /// <param name="productUpdateDTO">The updated product data</param>
        /// <param name="id">The ID of the product to update</param>
        /// <returns>The updated product</returns>
        /// <remarks>
        /// This is a partial update - only the provided fields will be updated.
        /// Fields that are not included in the request will retain their existing values.
        /// </remarks>
        /// <response code="200">Returns the updated product</response>
        /// <response code="400">If the product ID format is invalid or update data is invalid</response>
        /// <response code="404">If the product is not found</response>
        [HttpPatch("{id}", Name = "UpdateProductById")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProductById([FromBody] ProductUpdateDTO productUpdateDTO, string id)
        {
            var product = await _productService.UpdateProductByIdAsync(productUpdateDTO, id);
            return ApiResponse<ProductResponseDTO>.OkResult(product);
        }

        [HttpGet("GetProductsList", Name = "GetProductsList")]
        public async Task<IActionResult> GetProductsList()
        {
            var products = await _productService.GetProductsListAsync();
            return ApiResponse<IEnumerable<ProductResponseDTO>>.OkResult(products);
        }
    }
}