using AutoPile.DOMAIN.DTOs.Requests;
using AutoPile.DOMAIN.DTOs.Responses;
using AutoPile.DOMAIN.Models;
using AutoPile.DOMAIN.Models.Entities;
using AutoPile.SERVICE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoPile.API.Controllers
{
    /// <summary>
    /// Controller for managing shopping cart items
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class ShoppingCartItemController : ControllerBase
    {
        private readonly IShoppingCartItemService _shoppingCartItemService;
        private readonly ILogger<ShoppingCartItem> _logger;

        /// <summary>
        /// Initializes a new instance of the ShoppingCartItemController
        /// </summary>
        /// <param name="shoppingCartItemService">The shopping cart item service</param>
        /// <param name="logger">The logger instance</param>
        public ShoppingCartItemController(IShoppingCartItemService shoppingCartItemService, ILogger<ShoppingCartItem> logger)
        {
            _logger = logger;
            _shoppingCartItemService = shoppingCartItemService;
        }

        /// <summary>
        /// Retrieves a specific shopping cart item by its ID
        /// </summary>
        /// <param name="id">The ID of the shopping cart item to retrieve</param>
        /// <returns>The shopping cart item details</returns>
        /// <response code="200">Returns the shopping cart item</response>
        /// <response code="404">If the item is not found</response>
        /// <response code="401">If the user is not authorized</response>
        [HttpGet("{id}", Name = "GetShoppingCartItemById")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetShoppingCartItemById(int id)
        {
            var userId = HttpContext.Items["UserId"]?.ToString();
            var shoppingCartItemResponseDTO = await _shoppingCartItemService.GetShoppingCartItemById(id, userId);
            _logger.LogInformation("Shopping Cart Item retrieved successfully:{ShoppingCartItem}", shoppingCartItemResponseDTO);
            return ApiResponse<ShoppingCartItemResponseDTO>.OkResult(shoppingCartItemResponseDTO);
        }

        /// <summary>
        /// Creates a new shopping cart item
        /// </summary>
        /// <param name="shoppingCartItemResponseDTO">The shopping cart item information</param>
        /// <returns>The newly created shopping cart item</returns>
        /// <response code="200">Returns the newly created item</response>
        /// <response code="400">If the item is invalid</response>
        /// <response code="401">If the user is not authorized</response>
        [HttpPost("AddShoppingCartItem", Name = "AddShoppingCartItem")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CreateShoppingCartItem([FromBody] ShoppingCartItemRequestDto shoppingCartItemResponseDTO)
        {
            var userId = HttpContext.Items["UserId"]?.ToString();
            var shoppingCartItem = await _shoppingCartItemService.CreateShoppingCartItemAsync(shoppingCartItemResponseDTO, userId);
            _logger.LogInformation("Shopping Cart Item created successfully:{ShoppingCartItem}", shoppingCartItem);
            return ApiResponse<ShoppingCartItemResponseDTO>.OkResult(shoppingCartItem);
        }

        /// <summary>
        /// Deletes a specific shopping cart item
        /// </summary>
        /// <param name="id">The ID of the shopping cart item to delete</param>
        /// <returns>No content</returns>
        /// <response code="204">If the item was successfully deleted</response>
        /// <response code="404">If the item is not found</response>
        /// <response code="401">If the user is not authorized</response>
        [HttpDelete("{id}", Name = "DeleteShoppingCartItem")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> DeleteShoppingCartItem(int id)
        {
            var userId = HttpContext.Items["UserId"]?.ToString();
            await _shoppingCartItemService.DeleteShoppingCartItemAsync(id, userId);
            _logger.LogInformation("Shopping Cart Item deleted successfully with Id {id}", id);
            return NoContent();
        }

        /// <summary>
        /// Updates a specific shopping cart item
        /// </summary>
        /// <param name="updateShoppingCartItemDto">The updated shopping cart item information</param>
        /// <param name="id">The ID of the shopping cart item to update</param>
        /// <returns>A success message</returns>
        /// <response code="200">If the item was successfully updated</response>
        /// <response code="400">If the update information is invalid</response>
        /// <response code="404">If the item is not found</response>
        /// <response code="401">If the user is not authorized</response>
        [HttpPatch("{id}", Name = "UpdateShoppingCartItem")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> UpdateShoppingCartItem([FromBody] UpdateShoppingCartItemDto updateShoppingCartItemDto, int id)
        {
            var userId = HttpContext.Items["UserId"]?.ToString();
            await _shoppingCartItemService.UpdateShoppingCartItemAsync(updateShoppingCartItemDto, id, userId);
            _logger.LogInformation("Shopping Cart Item updated successfully with Id {ShoppingCartItemId}", id);
            return ApiResponse.OkResult("Resource updated successfully");
        }

        /// <summary>
        /// Deletes all shopping cart items for the authenticated user
        /// </summary>
        /// <remarks>
        /// This endpoint removes all items from the user's shopping cart.
        /// The user must be authenticated and have the "User" role to access this endpoint.
        /// </remarks>
        /// <response code="204">All shopping cart items were successfully deleted</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">User does not have the required role</response>
        /// <response code="404">User not found</response>
        /// <response code="500">Internal server error occurred</response>
        [HttpDelete]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> DeleteAllShoppingCartItems()
        {
            var userId = HttpContext.Items["UserId"]?.ToString();
            await _shoppingCartItemService.DeleteAllShoppingCartItemsAsync(userId);
            _logger.LogInformation("Whole shopping Cart Item deleted successfully with User Id {userId}", userId);
            return NoContent();
        }

        /// <summary>
        /// Retrieves specific shopping cart item list by User ID
        /// </summary>
        /// <returns>The shopping cart item list details</returns>
        /// <response code="200">Returns the shopping cart item list</response>
        /// <response code="404">If the item is not found</response>
        /// <response code="401">If the user is not authorized</response>
        [HttpGet("ShoppingCartItemList", Name = "ShoppingCartItemList")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetUserShoppingCartItemList()
        {
            var userId = HttpContext.Items["UserId"]?.ToString();
            var shoppingCartItemList = await _shoppingCartItemService.GetShoppingCartItemByUserId(userId);
            return ApiResponse<IEnumerable<ShoppingCartItemResponseDTO>>.OkResult(shoppingCartItemList);
        }
    }
}