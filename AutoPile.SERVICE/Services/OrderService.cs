using AutoMapper;
using AutoPile.DATA.Cache;
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
    private readonly IOrderCache _orderCache;

    public OrderService(AutoPileManagementDbContext autoPileManagementDbContext, IOrderCache orderCache, AutoPileMongoDbContext autoPileMongoDbContext, IMapper mapper, IInventoryQueueService inventoryQueueService)
    {
        _autoPileManagementDbContext = autoPileManagementDbContext;
        _autoPileMongoDbContext = autoPileMongoDbContext;
        _mapper = mapper;
        _inventoryQueueService = inventoryQueueService;
        _orderCache = orderCache;
    }

    public async Task<OrderResponseDTO> CreateOrderAsync(OrderCreateDTO orderCreateDTO, string applicationUserId)
    {
        using var transaction = await _autoPileManagementDbContext.Database.BeginTransactionAsync();
        try
        {
            var user = await _autoPileManagementDbContext.Users.FindAsync(applicationUserId)
                ?? throw new NotFoundException($"User with ID {applicationUserId} not found");

            var orderItems = new List<OrderItem>();
            decimal subtotal = 0;

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

                decimal productPrice = product.Price;
                if (product.ComparePrice.HasValue && product.ComparePrice.Value > 0)
                {
                    productPrice = product.ComparePrice.Value;
                }

                var itemTotal = productPrice * itemDTO.Quantity;

                var orderItem = new OrderItem
                {
                    ProductId = itemDTO.ProductId,
                    ProductName = product.Name,
                    ProductPrice = productPrice,
                    Quantity = itemDTO.Quantity,
                    TotalPrice = itemTotal
                };

                orderItems.Add(orderItem);
                subtotal += itemTotal;
            }

            var order = new Order
            {
                UserId = applicationUserId,
                OrderNumber = OrderNumberGenerator.GenerateSequentialOrderNumber(),
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                PaymentMethod = orderCreateDTO.PaymentMethod,
                ShippingAddress_Line1 = orderCreateDTO.ShippingAddress_Line1,
                ShippingAddress_Line2 = orderCreateDTO.ShippingAddress_Line2,
                ShippingAddress_City = orderCreateDTO.ShippingAddress_City,
                ShippingAddress_Country = orderCreateDTO.ShippingAddress_Country,
                ShippingAddress_State = orderCreateDTO.ShippingAddress_State,
                ShippingAddress_PostalCode = orderCreateDTO.ShippingAddress_PostalCode,
                DeliveryFee = orderCreateDTO.DeliveryFee,
                SubTotal = subtotal,
                TotalAmount = subtotal + orderCreateDTO.DeliveryFee,
                StripeSessionId = null,
                OrderItems = orderItems
            };

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

    public async Task<OrderResponseDTO> GetOrderByOrderIdAsync(string orderId, string applicationUserId)
    {
        _ = await _autoPileManagementDbContext.Users.FindAsync(applicationUserId)
                ?? throw new NotFoundException($"User with ID {applicationUserId} not found");
        var order = await _autoPileManagementDbContext.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.OrderNumber == orderId)
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

    public async Task CompleteOrderAsync(string applicationUserId, int orderId)
    {
        _ = await _autoPileManagementDbContext.Users.FindAsync(applicationUserId)
               ?? throw new NotFoundException($"User with ID {applicationUserId} not found");
        var order = await _autoPileManagementDbContext.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new NotFoundException($"Order with ID {orderId} not found");
        if (order.UserId != applicationUserId)
        {
            throw new ForbiddenException("You are not authorized to modify this order");
        }
        order.Status = OrderStatus.Success;
        order.PaymentStatus = PaymentStatus.Completed;
        await _autoPileManagementDbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<OrderResponseDTO>> GetUserOrdersAsync(string applicationUserId)
    {
        //var ordercache = await _orderCache.GetOrderAsync(applicationUserId);
        //if (ordercache != null)
        //{
        //    return ordercache;
        //}

        var orders = await _autoPileManagementDbContext.Orders.Include(o => o.OrderItems)
            .Where(o => o.UserId == applicationUserId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        //await _orderCache.SetOrderAsync(applicationUserId, _mapper.Map<IEnumerable<OrderResponseDTO>>(orders));

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
                decimal subtotal = 0;

                foreach (var itemDTO in orderUpdateDTO.OrderItems)
                {
                    if (!ObjectId.TryParse(itemDTO.ProductId, out ObjectId productObjectId))
                    {
                        throw new BadRequestException($"Invalid product ID format: {itemDTO.ProductId}");
                    }

                    var product = await _autoPileMongoDbContext.Products.FindAsync(productObjectId)
                        ?? throw new NotFoundException($"Product with Id {itemDTO.ProductId} not found");

                    decimal productPrice = product.Price;
                    if (product.ComparePrice.HasValue && product.ComparePrice.Value > 0)
                    {
                        productPrice = product.ComparePrice.Value;
                    }

                    var existingItem = order.OrderItems.FirstOrDefault(item => item.ProductId == itemDTO.ProductId);
                    if (existingItem != null)
                    {
                        if (itemDTO.Quantity > product.StockQuantity)
                        {
                            throw new BadRequestException($"Insufficient stock for product {product.Name}. Available: {product.StockQuantity}, Requested: {itemDTO.Quantity}");
                        }

                        existingItem.Quantity = itemDTO.Quantity;
                        existingItem.ProductName = product.Name;
                        existingItem.ProductPrice = productPrice;
                        existingItem.TotalPrice = productPrice * itemDTO.Quantity;

                        subtotal += existingItem.TotalPrice;
                    }
                    else
                    {
                        if (itemDTO.Quantity > product.StockQuantity)
                        {
                            throw new BadRequestException($"Insufficient stock for product {product.Name}. Available: {product.StockQuantity}, Requested: {itemDTO.Quantity}");
                        }

                        var newOrderItem = new OrderItem
                        {
                            OrderId = order.Id,
                            ProductId = itemDTO.ProductId,
                            ProductName = product.Name,
                            ProductPrice = productPrice,
                            Quantity = itemDTO.Quantity,
                            TotalPrice = productPrice * itemDTO.Quantity
                        };
                        order.OrderItems.Add(newOrderItem);

                        subtotal += newOrderItem.TotalPrice;
                    }
                }

                var itemsToRemove = order.OrderItems.Where(item => item.Quantity <= 0).ToList();
                foreach (var item in itemsToRemove)
                {
                    order.OrderItems.Remove(item);
                }

                var unchangedItems = order.OrderItems.Where(item => !orderUpdateDTO.OrderItems.Any(dto => dto.ProductId == item.ProductId));
                subtotal += unchangedItems.Sum(item => item.TotalPrice);

                order.SubTotal = subtotal;
                order.TotalAmount = subtotal + order.DeliveryFee;
            }

            await _autoPileManagementDbContext.SaveChangesAsync();

            var orderResponse = _mapper.Map<OrderResponseDTO>(order);
            await _orderCache.UpdateOrderAsync(orderResponse);

            await transaction.CommitAsync();

            return orderResponse;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}