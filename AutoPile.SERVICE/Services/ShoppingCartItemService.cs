using AutoMapper;
using AutoPile.DATA.Data;
using AutoPile.DATA.Exceptions;
using AutoPile.DOMAIN.DTOs.Requests;
using AutoPile.DOMAIN.DTOs.Responses;
using AutoPile.DOMAIN.Models.Entities;

namespace AutoPile.SERVICE.Services
{
    public class ShoppingCartItemService : IShoppingCartItemService
    {
        private readonly IMapper _mapper;
        private readonly AutoPileManagementDbContext _context;

        public ShoppingCartItemService(IMapper mapper, AutoPileManagementDbContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<ShoppingCartItemResponseDTO> CreateShoppingCartItemAsync(ShoppingCartItemRequestDto shoppingCartItemRequest, string? applicationUserId)
        {
            if (string.IsNullOrEmpty(applicationUserId))
            {
                throw new BadRequestException("User Id is null");
            }
            var user = await _context.Users.FindAsync(applicationUserId);
            if (user == null)
            {
                throw new NotFoundException($"User with ID {applicationUserId} not found");
            }
            var shoppingCartItem = _mapper.Map<ShoppingCartItem>(shoppingCartItemRequest);
            shoppingCartItem.UserId = applicationUserId;
            await _context.ShoppingCartItems.AddAsync(shoppingCartItem);
            await _context.SaveChangesAsync();
            var shoppingCartItemDTO = _mapper.Map<ShoppingCartItemResponseDTO>(shoppingCartItem);
            return shoppingCartItemDTO;
        }

        public async Task DeleteShoppingCartItemAsync(int shoppingCartItemId)
        {
            var shoppingCartItem = await _context.ShoppingCartItems.FindAsync(shoppingCartItemId) ?? throw new NotFoundException($"Shopping cart item with Id {shoppingCartItemId} is not found");
            _context.ShoppingCartItems.Remove(shoppingCartItem);
            await _context.SaveChangesAsync();
            return;
        }

        public async Task<ShoppingCartItemResponseDTO> GetShoppingCartItemById(int shoppingCartItemId)
        {
            var shoppingCartItem = await _context.ShoppingCartItems.FindAsync(shoppingCartItemId) ?? throw new NotFoundException($"Shopping cart item with Id {shoppingCartItemId} is not found");
            return _mapper.Map<ShoppingCartItemResponseDTO>(shoppingCartItem);
        }

        public async Task UpdateShoppingCartItemAsync(UpdateShoppingCartItemDto updateShoppingCartItemDto, int shoppingCartItemId, string? applicationUserId)
        {
            if (string.IsNullOrEmpty(applicationUserId))
            {
                throw new BadRequestException("User Id is null");
            }
            var user = await _context.Users.FindAsync(applicationUserId) ?? throw new NotFoundException($"User with ID {applicationUserId} not found");
            var shoppingCartItem = await _context.ShoppingCartItems.FindAsync(shoppingCartItemId) ?? throw new NotFoundException($"Shopping cart item with Id {shoppingCartItemId} is not found");
            if (shoppingCartItem.UserId != applicationUserId)
            {
                throw new UnauthorizedException("You are not authorized to modify this shopping cart item");
            }
            _mapper.Map(updateShoppingCartItemDto, shoppingCartItem);
            _context.ShoppingCartItems.Update(shoppingCartItem);
            await _context.SaveChangesAsync();
        }
    }
}