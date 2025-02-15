using AutoMapper;
using AutoPile.DATA.Data;
using AutoPile.DATA.Exceptions;
using AutoPile.DOMAIN.DTOs.Requests;
using AutoPile.DOMAIN.DTOs.Responses;
using AutoPile.DOMAIN.Models.Entities;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.SERVICE.Services
{
    public class OrderItemService : IOrderItemService
    {
        private readonly AutoPileManagementDbContext _autoPileManagementDbContext;
        private readonly AutoPileMongoDbContext _autoPileMongoDbContext;
        private readonly IMapper _mapper;

        public OrderItemService(AutoPileManagementDbContext autoPileManagementDbContext, AutoPileMongoDbContext autoPileMongoDbContext, IMapper mapper)
        {
            _autoPileManagementDbContext = autoPileManagementDbContext;
            _autoPileMongoDbContext = autoPileMongoDbContext;
            _mapper = mapper;
        }

        public async Task<OrderItemResponseDTO> CreateOrderItemAsync(OrderItemCreateDTO orderItemCreateDTO)
        {
            if (!ObjectId.TryParse(orderItemCreateDTO.ProductId, out ObjectId productObjectId))
            {
                throw new BadRequestException("Invalid product ID format");
            }
            var product = await _autoPileMongoDbContext.Products.FindAsync(productObjectId) ?? throw new NotFoundException($"Product with Id {orderItemCreateDTO.ProductId} not found");
            var orderItem = _mapper.Map<OrderItem>(orderItemCreateDTO);
            orderItem.TotalPrice = orderItem.ProductPrice * orderItem.Quantity;
            await _autoPileManagementDbContext.OrderItems.AddAsync(orderItem);
            await _autoPileManagementDbContext.SaveChangesAsync();
            return _mapper.Map<OrderItemResponseDTO>(orderItem);
        }

        public async Task<OrderItemResponseDTO> GetOrderItemById(string id)
        {
            var orderItem = await _autoPileManagementDbContext.OrderItems.FindAsync(id) ?? throw new NotFoundException($"OrderItem with id {id} is not found");
            var orderItemResponse = _mapper.Map<OrderItemResponseDTO>(orderItem);
            return orderItemResponse;
        }

        public async Task<OrderItemResponseDTO> UpdateOrderItem(OrderItemUpdateDTO orderItemUpdateDTO, string id)
        {
            var orderItem = await _autoPileManagementDbContext.OrderItems.FindAsync(id) ?? throw new NotFoundException($"OrderItem with id {id} is not found");
            _mapper.Map(orderItemUpdateDTO, orderItem);
            _autoPileManagementDbContext.OrderItems.Update(orderItem);
            await _autoPileManagementDbContext.SaveChangesAsync();
            return _mapper.Map<OrderItemResponseDTO>(orderItem);
        }

        public async Task DeleteOrderItem(string id)
        {
            var orderItem = await _autoPileManagementDbContext.OrderItems.FindAsync(id) ?? throw new NotFoundException($"OrderItem with id {id} is not found");
            _autoPileManagementDbContext.OrderItems.Remove(orderItem);
            await _autoPileManagementDbContext.SaveChangesAsync();
        }
    }
}