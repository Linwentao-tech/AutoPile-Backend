using AutoMapper;
using AutoPile.DATA.Cache;
using AutoPile.DATA.Data;
using AutoPile.DATA.Exceptions;
using AutoPile.DOMAIN.DTOs.Requests;
using AutoPile.DOMAIN.DTOs.Responses;
using AutoPile.DOMAIN.Models.Entities;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;

namespace AutoPile.SERVICE.Services
{
    public class ShoppingCartItemService : IShoppingCartItemService
    {
        private readonly IMapper _mapper;
        private readonly AutoPileManagementDbContext _context;
        private readonly AutoPileMongoDbContext _mongoContext;
        private readonly IRedisShoppingCartCache _redisShoppingCartCache;

        public ShoppingCartItemService(IMapper mapper, AutoPileManagementDbContext context, AutoPileMongoDbContext mongoContext, IRedisShoppingCartCache redisShoppingCartCache)
        {
            _mapper = mapper;
            _context = context;
            _mongoContext = mongoContext;
            _redisShoppingCartCache = redisShoppingCartCache;
        }

        public async Task<ShoppingCartItemResponseDTO> CreateShoppingCartItemAsync(ShoppingCartItemRequestDto shoppingCartItemRequest, string applicationUserId)
        {
            if (string.IsNullOrEmpty(applicationUserId))
            {
                throw new BadRequestException("User Id is null");
            }
            var user = await _context.Users.FindAsync(applicationUserId) ?? throw new NotFoundException($"User with ID {applicationUserId} not found");
            if (!ObjectId.TryParse(shoppingCartItemRequest.ProductId, out ObjectId productObjectId))
            {
                throw new BadRequestException("Invalid product ID format");
            }
            _ = await _mongoContext.Products.FindAsync(productObjectId)
                ?? throw new NotFoundException($"Product with ID {shoppingCartItemRequest.ProductId} not found");

            var existedCartItem = await _context.ShoppingCartItems.FirstOrDefaultAsync(s => s.ProductId == shoppingCartItemRequest.ProductId && s.UserId == applicationUserId);
            if (existedCartItem != null)
            {
                existedCartItem.Quantity += shoppingCartItemRequest.Quantity;
                if (existedCartItem.Quantity <= 0)
                {
                    // Directly remove from database
                    _context.ShoppingCartItems.Remove(existedCartItem);
                    await _context.SaveChangesAsync();

                    // Remove from Redis cache
                    await _redisShoppingCartCache.RemoveItemAsync(applicationUserId, existedCartItem.Id);

                    // Check if cart is empty and clear Redis cache if needed
                    var cart = await _redisShoppingCartCache.GetUserCartAsync(applicationUserId);
                    if (cart.Count == 0)
                    {
                        await _redisShoppingCartCache.ClearCartAsync(applicationUserId);
                    }

                    return _mapper.Map<ShoppingCartItemResponseDTO>(existedCartItem);
                }

                await _context.SaveChangesAsync();
                await _redisShoppingCartCache.SetItemAsync(existedCartItem);
                return _mapper.Map<ShoppingCartItemResponseDTO>(existedCartItem);
            }
            else
            {
                // Add validation for negative quantity when creating new cart item
                if (shoppingCartItemRequest.Quantity <= 0)
                {
                    throw new BadRequestException("Cannot add item with zero or negative quantity");
                }

                var shoppingCartItem = _mapper.Map<ShoppingCartItem>(shoppingCartItemRequest);
                shoppingCartItem.UserId = applicationUserId;
                shoppingCartItem.CreatedAt = DateTime.UtcNow;
                await _context.ShoppingCartItems.AddAsync(shoppingCartItem);
                await _context.SaveChangesAsync();
                await _redisShoppingCartCache.SetItemAsync(shoppingCartItem);
                var shoppingCartItemDTO = _mapper.Map<ShoppingCartItemResponseDTO>(shoppingCartItem);
                return shoppingCartItemDTO;
            }
        }

        public async Task DeleteShoppingCartItemAsync(int shoppingCartItemId, string applicationUserId)
        {
            var shopppingCartCachedItem = await _redisShoppingCartCache.GetItemAsync(applicationUserId, shoppingCartItemId);
            ShoppingCartItem shoppingCartItem;
            if (shopppingCartCachedItem != null)
            {
                shoppingCartItem = shopppingCartCachedItem;
            }
            else
            {
                shoppingCartItem = await _context.ShoppingCartItems.FindAsync(shoppingCartItemId) ?? throw new NotFoundException($"Shopping cart item with Id {shoppingCartItemId} is not found");
            }

            if (shoppingCartItem.UserId != applicationUserId)
            {
                throw new ForbiddenException("You are not authorized to delete this shopping cart item");
            }
            _context.ShoppingCartItems.Remove(shoppingCartItem);
            await _context.SaveChangesAsync();

            await _redisShoppingCartCache.RemoveItemAsync(applicationUserId, shoppingCartItemId);

            var shoppingCart = await _redisShoppingCartCache.GetUserCartAsync(applicationUserId);
            if (shoppingCart.Count == 0)
            {
                await _redisShoppingCartCache.ClearCartAsync(applicationUserId);
            }
            return;
        }

        public async Task DeleteAllShoppingCartItemsAsync(string applicationUserId)
        {
            var user = await _context.Users.FindAsync(applicationUserId) ?? throw new NotFoundException($"User with ID {applicationUserId} not found");
            var userShoppingCartItems = _context.ShoppingCartItems.Where(item => item.UserId == applicationUserId);

            if (userShoppingCartItems.Count() == 0)
            {
                throw new BadRequestException($"User {applicationUserId}'s Shopping Cart is already emmpty");
            }

            if (userShoppingCartItems.Any(item => item.UserId != applicationUserId))
            {
                throw new ForbiddenException("You are not authorized to delete some of these shopping cart items");
            }

            _context.RemoveRange(userShoppingCartItems);
            await _context.SaveChangesAsync(true);

            var cart = await _redisShoppingCartCache.GetUserCartAsync(applicationUserId);
            if (cart.Count == 0)
            {
                return;
            }
            await _redisShoppingCartCache.ClearCartAsync(applicationUserId);
        }

        public async Task<ShoppingCartItemResponseDTO> GetShoppingCartItemById(int shoppingCartItemId, string applicationUserId)
        {
            var shopppingCartCachedItem = await _redisShoppingCartCache.GetItemAsync(applicationUserId, shoppingCartItemId);
            ShoppingCartItem shoppingCartItem;
            if (shopppingCartCachedItem != null)
            {
                shoppingCartItem = shopppingCartCachedItem;
            }
            else
            {
                shoppingCartItem = await _context.ShoppingCartItems.FindAsync(shoppingCartItemId) ?? throw new NotFoundException($"Shopping cart item with Id {shoppingCartItemId} is not found");
            }

            if (shoppingCartItem.UserId != applicationUserId)
            {
                throw new ForbiddenException("You are not authorized to see this shopping cart item");
            }

            await _redisShoppingCartCache.SetItemAsync(shoppingCartItem);
            return _mapper.Map<ShoppingCartItemResponseDTO>(shoppingCartItem);
        }

        public async Task UpdateShoppingCartItemAsync(UpdateShoppingCartItemDto updateShoppingCartItemDto, int shoppingCartItemId, string? applicationUserId)
        {
            if (string.IsNullOrEmpty(applicationUserId))
            {
                throw new BadRequestException("User Id is null");
            }
            var user = await _context.Users.FindAsync(applicationUserId) ?? throw new NotFoundException($"User with ID {applicationUserId} not found");

            var shoppingCartCachedItem = await _redisShoppingCartCache.GetItemAsync(applicationUserId, shoppingCartItemId);
            ShoppingCartItem shoppingCartItem;
            if (shoppingCartCachedItem != null)
            {
                shoppingCartItem = shoppingCartCachedItem;
            }
            else
            {
                shoppingCartItem = await _context.ShoppingCartItems.FindAsync(shoppingCartItemId) ?? throw new NotFoundException($"Shopping cart item with Id {shoppingCartItemId} is not found");
            }

            if (shoppingCartItem.UserId != applicationUserId)
            {
                throw new ForbiddenException("You are not authorized to modify this shopping cart item");
            }
            _mapper.Map(updateShoppingCartItemDto, shoppingCartItem);
            _context.ShoppingCartItems.Update(shoppingCartItem);
            await _context.SaveChangesAsync();

            await _redisShoppingCartCache.SetItemAsync(shoppingCartItem);
        }
    }
}