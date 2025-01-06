using AutoMapper;
using AutoPile.DATA.Data;
using AutoPile.DATA.Exceptions;
using AutoPile.DOMAIN.DTOs.Requests;
using MongoDB.Bson;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoPile.SERVICE.Services
{
    public class PaymentService
    {
        private readonly AutoPileManagementDbContext _autoPileManagementDbContext;
        private readonly AutoPileMongoDbContext _autoPileMongoDbContext;
        private readonly IMapper _mapper;

        public PaymentService(AutoPileManagementDbContext autoPileManagementDbContext, AutoPileMongoDbContext autoPileMongoDbContext, IMapper mapper)
        {
            _autoPileManagementDbContext = autoPileManagementDbContext;
            _autoPileMongoDbContext = autoPileMongoDbContext;
            _mapper = mapper;
        }

        public async Task<PaymentIntent> PaymentIntentCreateAsync(PaymentIntentCreate paymentIntentCreate)
        {
            if (paymentIntentCreate == null)
            {
                throw new BadRequestException("No products");
            }

            var totalAmount = await CalculateTotalAmountAsync(paymentIntentCreate.Items);

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(totalAmount * 100),
                Currency = "aud",
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
            };

            var service = new PaymentIntentService();
            return await service.CreateAsync(options);
        }

        private async Task<decimal> CalculateTotalAmountAsync(Item[] items)
        {
            decimal totalAmount = 0;
            foreach (var item in items)
            {
                var productId = item.ProductId;
                var quantity = item.Quantity;

                if (!ObjectId.TryParse(productId, out ObjectId productObjectId))
                {
                    throw new BadRequestException("Invalid product ID format");
                }

                var product = await _autoPileMongoDbContext.Products.FindAsync(productObjectId)
                    ?? throw new NotFoundException($"Product with Id {productId} not found");

                decimal price = (product.ComparePrice ?? product.Price) * quantity;
                totalAmount += price;
            }
            return totalAmount;
        }
    }
}