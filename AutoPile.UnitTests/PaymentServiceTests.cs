using AutoPile.DATA.Data;
using AutoPile.DOMAIN.DTOs.Requests;
using AutoPile.DOMAIN.Models.Entities;
using AutoPile.SERVICE.Services;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using Moq;
using Stripe;
using FluentAssertions;
using Xunit;

namespace AutoPile.UnitTests
{
    public class PaymentServiceTests
    {
        private readonly Mock<AutoPileMongoDbContext> _mongoDbContextMock;
        private readonly Mock<IStripeService> _stripeServiceMock;
        private readonly PaymentService _paymentService;

        public PaymentServiceTests()
        {
            _mongoDbContextMock = new Mock<AutoPileMongoDbContext>(new DbContextOptionsBuilder<AutoPileMongoDbContext>().Options);
            _stripeServiceMock = new Mock<IStripeService>();
            _paymentService = new PaymentService(_mongoDbContextMock.Object, _stripeServiceMock.Object);
        }

        [Fact]
        public async Task CreatePaymentIntent_ValidItemListInput_ReturnSecretObject()
        {
            // Arrange
            var productId = ObjectId.GenerateNewId();
            var product = new DOMAIN.Models.Entities.Product { Id = productId, Price = 100 };
            var item = new Item { ProductId = productId.ToString(), Quantity = 2 };
            var itemList = new PaymentIntentCreate { Items = [item] };

            _mongoDbContextMock.Setup(x => x.Products.FindAsync(productId))
                .ReturnsAsync(product);

            var mockPaymentIntent = new PaymentIntent
            {
                ClientSecret = "mock_client_secret_test"
            };

            _stripeServiceMock.Setup(x => x.CreatePaymentIntentAsync(It.IsAny<long>(), It.IsAny<string>()))
                .ReturnsAsync(mockPaymentIntent);

            // Act
            var result = await _paymentService.PaymentIntentCreateAsync(itemList);

            // Assert
            Assert.NotNull(result);
            result.ClientSecret.Should().Be("mock_client_secret_test");
        }
    }
}