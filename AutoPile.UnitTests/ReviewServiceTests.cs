using AutoMapper;
using AutoPile.DATA.Cache;
using AutoPile.DATA.Data;
using AutoPile.DOMAIN.DTOs.Requests;
using AutoPile.DOMAIN.DTOs.Responses;
using AutoPile.DOMAIN.Interface;
using AutoPile.DOMAIN.Models.Entities;
using AutoPile.SERVICE.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace AutoPile.UnitTests
{
    public class ReviewServiceTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IBlobService> _blobServiceMock;
        private readonly Mock<AutoPileMongoDbContext> _mongoDbContextMock;
        private readonly Mock<AutoPileManagementDbContext> _managementDbContextMock;
        private readonly Mock<IReviewsCache> _reviewsCacheMock;
        private readonly ReviewService _reviewService;
        private readonly Mock<DbSet<Review>> _reviewsDbSetMock;
        private readonly Mock<DbSet<Product>> _productsDbSetMock;
        private readonly Mock<DbSet<ApplicationUser>> _usersDbSetMock;

        public ReviewServiceTests()
        {
            _mapperMock = new Mock<IMapper>();
            _blobServiceMock = new Mock<IBlobService>();
            _mongoDbContextMock = new Mock<AutoPileMongoDbContext>(new DbContextOptionsBuilder<AutoPileMongoDbContext>().Options);
            _managementDbContextMock = new Mock<AutoPileManagementDbContext>(new DbContextOptions<AutoPileManagementDbContext>());
            _reviewsCacheMock = new Mock<IReviewsCache>();
            _reviewsDbSetMock = new Mock<DbSet<Review>>();
            _productsDbSetMock = new Mock<DbSet<Product>>();
            _usersDbSetMock = new Mock<DbSet<ApplicationUser>>();

            _mongoDbContextMock.Setup(x => x.Reviews).Returns(_reviewsDbSetMock.Object);
            _mongoDbContextMock.Setup(x => x.Products).Returns(_productsDbSetMock.Object);
            _managementDbContextMock.Setup(x => x.Users).Returns(_usersDbSetMock.Object);

            _reviewService = new ReviewService(
                _mapperMock.Object,
                _blobServiceMock.Object,
                _mongoDbContextMock.Object,
                _managementDbContextMock.Object,
                _reviewsCacheMock.Object
            );
            var userId = "testUserId";
            var user = new ApplicationUser { Id = userId };
            _managementDbContextMock.Setup(x => x.Users.FindAsync(userId))
                .ReturnsAsync(user);
        }

        [Fact]
        public async Task CreateReviewAsync_ValidInput_ReturnsReviewResponseDTO()
        {
            // Arrange

            var productId = ObjectId.GenerateNewId().ToString();

            var reviewCreateDto = new ReviewCreateDTO
            {
                ProductId = productId,
                Title = "Test Review",
                Content = "Test Content",
                Subtitle = "Test Subtitle",
                Rating = 5
            };

            var product = new Product { Id = ObjectId.Parse(productId) };

            var review = new Review
            {
                Id = ObjectId.GenerateNewId(),
                Title = reviewCreateDto.Title,
                Content = reviewCreateDto.Content,
                Subtitle = reviewCreateDto.Subtitle,
                Rating = reviewCreateDto.Rating,
                UserId = "testUserId",
                ProductId = ObjectId.Parse(productId)
            };

            var reviewResponseDto = new ReviewResponseDTO
            {
                Id = review.Id.ToString(),
                Title = review.Title,
                Content = review.Content,
                Subtitle = review.Subtitle,
                Rating = review.Rating,
                UserId = review.UserId,
                ProductId = review.ProductId.ToString(),
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.CreatedAt
            };

            _mongoDbContextMock.Setup(x => x.Products.FindAsync(ObjectId.Parse(productId)))
                .ReturnsAsync(product);

            _mapperMock.Setup(x => x.Map<Review>(reviewCreateDto))
                .Returns(review);

            _mapperMock.Setup(x => x.Map<ReviewResponseDTO>(review))
                .Returns(reviewResponseDto);

            // Act
            var result = await _reviewService.CreateReviewAsync(reviewCreateDto, "testUserId");

            // Assert
            Assert.NotNull(result);
            result.Should().BeEquivalentTo(reviewResponseDto);

            _mongoDbContextMock.Verify(x => x.Reviews.AddAsync(review, default), Times.Once);
            _mongoDbContextMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task GetReview_ValidInput_ReturnResponseDTO()
        {
            //Arrange
            var productId = ObjectId.GenerateNewId();
            var review = new Review
            {
                Id = ObjectId.GenerateNewId(),
                ProductId = productId,
                Title = "Test Review",
                Content = "Test Content",
                Subtitle = "Test Subtitle",
                UserId = "testUserId",
                Rating = 5
            };

            var reviewResponseDto = new ReviewResponseDTO
            {
                Id = review.Id.ToString(),
                Title = review.Title,
                Content = review.Content,
                Subtitle = review.Subtitle,
                Rating = review.Rating,
                UserId = review.UserId,
                ProductId = review.ProductId.ToString(),
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.CreatedAt
            };
            _mongoDbContextMock.Setup(x => x.Reviews.FindAsync(review.Id)).ReturnsAsync(review);
            _mapperMock.Setup(x => x.Map<ReviewResponseDTO>(review))
        .Returns(reviewResponseDto);

            //Act
            var result = await _reviewService.GetReviewByIdAsync(review.Id.ToString());

            //Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(reviewResponseDto);
        }

        [Fact]
        public async Task DeleteReview_ValidInput_NoReturn()
        {
            //Arrange
            var productId = ObjectId.GenerateNewId();
            var review = new Review
            {
                Id = ObjectId.GenerateNewId(),
                ProductId = productId,
                Title = "Test Review",
                Content = "Test Content",
                Subtitle = "Test Subtitle",
                UserId = "testUserId",
                Rating = 5,
                ImageUrl = "test-image.jpg"
            };

            var reviewResponseDto = new ReviewResponseDTO
            {
                Id = review.Id.ToString(),
                Title = review.Title,
                Content = review.Content,
                Subtitle = review.Subtitle,
                Rating = review.Rating,
                UserId = review.UserId,
                ProductId = review.ProductId.ToString(),
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.CreatedAt
            };
            _mongoDbContextMock.Setup(x => x.Reviews.FindAsync(review.Id)).ReturnsAsync(review);
            _mapperMock.Setup(x => x.Map<ReviewResponseDTO>(review))
        .Returns(reviewResponseDto);
            //Act
            await _reviewService.DeleteReviewAsync(review.Id.ToString(), "testUserId");

            //Assert
            _mongoDbContextMock.Verify(x => x.Remove(review), Times.Once);
            _blobServiceMock.Verify(x => x.DeleteImageAsync(review.ImageUrl), Times.Once);
        }
    }
}