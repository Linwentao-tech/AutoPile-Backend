using AutoPile.DATA.Data;
using AutoPile.DATA.Exceptions;
using AutoPile.DOMAIN.DTOs.Requests;
using MongoDB.Bson;
using Stripe;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoPile.SERVICE.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AutoPileMongoDbContext _autoPileMongoDbContext;
        private readonly IStripeService _stripeService;

        public PaymentService(AutoPileMongoDbContext autoPileMongoDbContext, IStripeService stripeService)
        {
            _autoPileMongoDbContext = autoPileMongoDbContext;
            _stripeService = stripeService;
        }

        public async Task<PaymentIntent> PaymentIntentCreateAsync(PaymentIntentCreate paymentIntentCreate)
        {
            if (paymentIntentCreate == null)
            {
                throw new BadRequestException("No products");
            }

            var totalAmount = await CalculateTotalAmountAsync(paymentIntentCreate.Items);
            return await _stripeService.CreatePaymentIntentAsync((long)(totalAmount * 100), "aud");
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
            return totalAmount + 10;
        }
    }
}