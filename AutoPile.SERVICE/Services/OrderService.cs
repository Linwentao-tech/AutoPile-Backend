using AutoMapper;
using AutoPile.DATA.Data;
using AutoPile.DATA.Exceptions;
using AutoPile.DOMAIN.DTOs.Requests;
using AutoPile.DOMAIN.DTOs.Responses;
using AutoPile.DOMAIN.Enum;
using AutoPile.DOMAIN.Models.Entities;
using AutoPile.SERVICE.Services;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;

public class OrderService : IOrderService
{
    private readonly AutoPileManagementDbContext _autoPileManagementDbContext;
    private readonly AutoPileMongoDbContext _autoPileMongoDbContext;
    private readonly IMapper _mapper;
    private readonly IInventoryQueueService _inventoryQueueService;

    public OrderService(AutoPileManagementDbContext autoPileManagementDbContext, AutoPileMongoDbContext autoPileMongoDbContext, IMapper mapper, IInventoryQueueService inventoryQueueService)
    {
        _autoPileManagementDbContext = autoPileManagementDbContext;
        _autoPileMongoDbContext = autoPileMongoDbContext;
        _mapper = mapper;
        _inventoryQueueService = inventoryQueueService;
    }

    public async Task<OrderResponseDTO> CreateOrderAsync(OrderCreateDTO orderCreateDTO, string applicationUserId)
    {
        using var transaction = await _autoPileManagementDbContext.Database.BeginTransactionAsync();
        try
        {
            var user = await _autoPileManagementDbContext.Users.FindAsync(applicationUserId)
                ?? throw new NotFoundException($"User with ID {applicationUserId} not found");

            foreach (var itemDTO in orderCreateDTO.OrderItems)
            {
                if (!ObjectId.TryParse(itemDTO.ProductId, out ObjectId productObjectId))
                {
                    throw new BadRequestException("Invalid product ID format");
                }
                var product = await _autoPileMongoDbContext.Products.FindAsync(productObjectId)
                    ?? throw new NotFoundException($"Product with Id {itemDTO.ProductId} not found");
                if (product.StockQuantity < itemDTO.Quantity)
                {
                    throw new BadRequestException($"Insufficient stock for product {product.Name}. Available: {product.StockQuantity}, Requested: {itemDTO.Quantity}");
                }
            }

            var order = _mapper.Map<Order>(orderCreateDTO);

            order.UserId = applicationUserId;
            order.OrderNumber = OrderNumberGenerator.GenerateSequentialOrderNumber();
            order.OrderDate = DateTime.UtcNow;
            order.Status = OrderStatus.Pending;
            order.PaymentStatus = PaymentStatus.Pending;
            order.StripeSessionId = null;

            order.SubTotal = orderCreateDTO.OrderItems.Sum(item => item.ProductPrice * item.Quantity);
            order.TotalAmount = order.SubTotal + order.DeliveryFee;

            order.OrderItems = orderCreateDTO.OrderItems.Select(itemDTO => new OrderItem
            {
                ProductId = itemDTO.ProductId,
                ProductName = itemDTO.ProductName,
                ProductPrice = itemDTO.ProductPrice,
                Quantity = itemDTO.Quantity,
                TotalPrice = itemDTO.ProductPrice * itemDTO.Quantity
            }).ToList();

            await _autoPileManagementDbContext.Orders.AddAsync(order);
            await _autoPileManagementDbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            await _inventoryQueueService.QueueOrderItemMessage(order.OrderItems);

            return _mapper.Map<OrderResponseDTO>(order);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<OrderResponseDTO> GetOrderByIdAsync(int orderId, string applicationUserId)
    {
        _ = await _autoPileManagementDbContext.Users.FindAsync(applicationUserId)
                ?? throw new NotFoundException($"User with ID {applicationUserId} not found");
        var order = await _autoPileManagementDbContext.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new NotFoundException($"Order with ID {orderId} not found");
        if (order.UserId != applicationUserId)
        {
            throw new ForbiddenException("You are not authorized to view this order");
        }

        return _mapper.Map<OrderResponseDTO>(order);
    }

    public async Task DeleteOrderAsync(int orderId, string userId)
    {
        using var transaction = await _autoPileManagementDbContext.Database.BeginTransactionAsync();
        try
        {
            var order = await _autoPileManagementDbContext.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId)
                ?? throw new NotFoundException($"Order with ID {orderId} not found");

            if (order.UserId != userId)
            {
                throw new ForbiddenException("You are not authorized to delete this order");
            }

            if (order.Status == OrderStatus.Success)
            {
                throw new BadRequestException("Cannot delete a completed order");
            }

            _autoPileManagementDbContext.OrderItems.RemoveRange(order.OrderItems);
            _autoPileManagementDbContext.Orders.Remove(order);

            await _autoPileManagementDbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IEnumerable<OrderResponseDTO>> GetUserOrdersAsync(string applicationUserId)
    {
        var orders = await _autoPileManagementDbContext.Orders.Include(o => o.OrderItems)
            .Where(o => o.UserId == applicationUserId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return _mapper.Map<IEnumerable<OrderResponseDTO>>(orders);
    }

    public async Task<OrderResponseDTO> UpdateOrderAsync(OrderUpdateDTO orderUpdateDTO, int orderId, string userId)
    {
        using var transaction = await _autoPileManagementDbContext.Database.BeginTransactionAsync();
        try
        {
            var order = await _autoPileManagementDbContext.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId)
                ?? throw new NotFoundException($"Order with ID {orderId} not found");

            if (order.UserId != userId)
            {
                throw new ForbiddenException("You are not authorized to update this order");
            }

            if (order.Status == OrderStatus.Success)
            {
                throw new BadRequestException("Cannot update a completed order");
            }

            if (orderUpdateDTO.Status.HasValue)
                order.Status = orderUpdateDTO.Status.Value;

            if (!string.IsNullOrWhiteSpace(orderUpdateDTO.PaymentMethod))
                order.PaymentMethod = orderUpdateDTO.PaymentMethod;

            if (!string.IsNullOrWhiteSpace(orderUpdateDTO.ShippingAddress_Line1))
                order.ShippingAddress_Line1 = orderUpdateDTO.ShippingAddress_Line1;

            if (!string.IsNullOrWhiteSpace(orderUpdateDTO.ShippingAddress_Line2))
                order.ShippingAddress_Line2 = orderUpdateDTO.ShippingAddress_Line2;

            if (!string.IsNullOrWhiteSpace(orderUpdateDTO.ShippingAddress_City))
                order.ShippingAddress_City = orderUpdateDTO.ShippingAddress_City;

            if (!string.IsNullOrWhiteSpace(orderUpdateDTO.ShippingAddress_Country))
                order.ShippingAddress_Country = orderUpdateDTO.ShippingAddress_Country;

            if (!string.IsNullOrWhiteSpace(orderUpdateDTO.ShippingAddress_State))
                order.ShippingAddress_State = orderUpdateDTO.ShippingAddress_State;

            if (!string.IsNullOrWhiteSpace(orderUpdateDTO.ShippingAddress_PostalCode))
                order.ShippingAddress_PostalCode = orderUpdateDTO.ShippingAddress_PostalCode;

            if (orderUpdateDTO.OrderItems != null && orderUpdateDTO.OrderItems.Any())
            {
                foreach (var itemDTO in orderUpdateDTO.OrderItems)
                {
                    var existingItem = order.OrderItems.FirstOrDefault(item => item.ProductId == itemDTO.ProductId);

                    if (existingItem != null)
                    {
                        existingItem.Quantity = itemDTO.Quantity;
                        existingItem.TotalPrice = itemDTO.ProductPrice * itemDTO.Quantity;
                    }
                    else
                    {
                        var newOrderItem = new OrderItem
                        {
                            OrderId = order.Id,
                            ProductId = itemDTO.ProductId,
                            ProductName = itemDTO.ProductName,
                            ProductPrice = itemDTO.ProductPrice,
                            Quantity = itemDTO.Quantity,
                            TotalPrice = itemDTO.ProductPrice * itemDTO.Quantity
                        };
                        order.OrderItems.Add(newOrderItem);
                    }
                }

                foreach (var item in order.OrderItems.Where(item => item.Quantity <= 0).ToList())
                {
                    order.OrderItems.Remove(item);
                }
            }

            order.SubTotal = order.OrderItems.Sum(item => item.TotalPrice);
            order.TotalAmount = order.SubTotal + order.DeliveryFee;

            await _autoPileManagementDbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return _mapper.Map<OrderResponseDTO>(order);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}