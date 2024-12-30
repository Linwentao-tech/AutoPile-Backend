using AutoPile.DOMAIN.DTOs.Requests;
using AutoPile.DOMAIN.DTOs.Responses;
using AutoPile.DOMAIN.Models.Entities;
using AutoPile.SERVICE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoPile.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ShoppingCartItem : ControllerBase
    {
        private readonly IShoppingCartItemService _shoppingCartItemService;
        private readonly ILogger<ShoppingCartItem> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShoppingCartItem(IShoppingCartItemService shoppingCartItemService, ILogger<ShoppingCartItem> logger, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _shoppingCartItemService = shoppingCartItemService;
            _userManager = userManager;
        }

        [HttpGet("{id}", Name = "GetShoppingCartItemById")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetShoppingCartItemById(int id)
        {
            var userId = HttpContext.Items["UserId"]?.ToString();
            var shoppingCartItemResponseDTO = await _shoppingCartItemService.GetShoppingCartItemById(id, userId);
            _logger.LogInformation("Shopping Cart Item retrieved successfully:{ShoppingCartItem}", shoppingCartItemResponseDTO);
            return Ok(shoppingCartItemResponseDTO);
        }

        [HttpPost("AddShoppingCartItem", Name = "AddShoppingCartItem")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CreateShoppingCartItem([FromBody] ShoppingCartItemRequestDto shoppingCartItemResponseDTO)
        {
            var userId = HttpContext.Items["UserId"]?.ToString();
            var shoppingCartItem = await _shoppingCartItemService.CreateShoppingCartItemAsync(shoppingCartItemResponseDTO, userId);
            _logger.LogInformation("Shopping Cart Item created successfully:{ShoppingCartItem}", shoppingCartItemResponseDTO);
            return Ok(shoppingCartItem);
        }

        [HttpDelete("{id}", Name = "DeleteShoppingCartItem")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> DeleteShoppingCartItem(int id)
        {
            var userId = HttpContext.Items["UserId"]?.ToString();
            await _shoppingCartItemService.DeleteShoppingCartItemAsync(id, userId);
            _logger.LogInformation("Shopping Cart Item deleted successfully with Id {id}", id);
            return NoContent();
        }

        [HttpPatch("{id}", Name = "UpdateShoppingCartItem")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> UpdateShoppingCartItem([FromBody] UpdateShoppingCartItemDto updateShoppingCartItemDto, int shoppingCartItemId)
        {
            var userId = HttpContext.Items["UserId"]?.ToString();
            await _shoppingCartItemService.UpdateShoppingCartItemAsync(updateShoppingCartItemDto, shoppingCartItemId, userId);
            _logger.LogInformation("Shopping Cart Item updated successfully with Id {id}", shoppingCartItemId);
            return NoContent();
        }
    }
}