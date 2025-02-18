using AutoMapper;
using AutoPile.DATA.Cache;
using AutoPile.DATA.Exceptions;
using AutoPile.DOMAIN.DTOs.Requests;
using AutoPile.DOMAIN.DTOs.Responses;
using AutoPile.DOMAIN.Models.Entities;
using AutoPile.SERVICE.Services;
using AutoPile.SERVICE.Utilities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Resend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.UnitTests
{
    public class AuthServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
        private readonly Mock<IResend> _resendMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IEmailQueueService> _emailQueueServiceMock;
        private readonly Mock<IUserInfoCache> _userInfoCacheMock;
        private readonly Mock<ILogger<IAuthService>> _loggerMock;
        private readonly AuthService _authServiceMock;

        public AuthServiceTests()
        {
            var storeMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
           storeMock.Object,
           null, // IOptions<IdentityOptions>
           null, // IPasswordHasher<ApplicationUser>
           null, // IEnumerable<IUserValidator<ApplicationUser>>
           null, // IEnumerable<IPasswordValidator<ApplicationUser>>
           null, // ILookupNormalizer
           null, // IdentityErrorDescriber
           null, // IServiceProvider
           null  // ILogger<UserManager<ApplicationUser>>
       );
            _mapperMock = new Mock<IMapper>();
            _userInfoCacheMock = new Mock<IUserInfoCache>();
            _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
            _configurationMock = new Mock<IConfiguration>();
            _resendMock = new Mock<IResend>();
            _emailQueueServiceMock = new Mock<IEmailQueueService>();
            _loggerMock = new Mock<ILogger<IAuthService>>();
            _authServiceMock = new AuthService(_userManagerMock.Object, _loggerMock.Object, _emailQueueServiceMock.Object,
                _configurationMock.Object, _mapperMock.Object, _userInfoCacheMock.Object, _jwtTokenGeneratorMock.Object, _resendMock.Object);
        }

        [Fact]
        public async Task SignUpUser_ValidInput_ReturnUserResponseDTO()
        {
            // Arrange
            var password = "TestPassword";
            var userSignUpDTO = new UserSignupDTO()
            {
                FirstName = "firstName",
                LastName = "lastName",
                UserName = "TestUserName",
                PhoneNumber = "TestPhoneNumber",
                Email = "TestEmail",
                Password = password
            };

            _mapperMock.Setup(m => m.Map<ApplicationUser>(It.IsAny<UserSignupDTO>()))
                .Returns(new ApplicationUser
                {
                    UserName = userSignUpDTO.UserName,
                    Email = userSignUpDTO.Email,
                    FirstName = userSignUpDTO.FirstName,
                    LastName = userSignUpDTO.LastName,
                    PhoneNumber = userSignUpDTO.PhoneNumber,
                    Id = "TestId"
                });

            _mapperMock.Setup(m => m.Map<UserResponseDTO>(It.IsAny<ApplicationUser>()))
                .Returns(new UserResponseDTO
                {
                    UserName = userSignUpDTO.UserName,
                    Email = userSignUpDTO.Email,
                    Id = "TestId"
                });

            _userManagerMock.Setup(u => u.FindByEmailAsync(userSignUpDTO.Email))
                .ReturnsAsync(null as ApplicationUser);

            _userManagerMock.Setup(u => u.Users)
                .Returns(new List<ApplicationUser>().AsQueryable());

            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), password))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(u => u.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(["User"]);

            _jwtTokenGeneratorMock.Setup(t => t.GenerateJwtToken(It.IsAny<ApplicationUser>()))
                .Returns("test_access_token");

            // Act
            var result = await _authServiceMock.SignupUserAsync(userSignUpDTO);

            // Assert
            result.Should().NotBeNull();
            var (userResponse, accessToken, refreshToken) = result;

            userResponse.Should().NotBeNull();
            userResponse.Should().BeOfType<UserResponseDTO>();
            userResponse.UserName.Should().Be(userSignUpDTO.UserName);
            userResponse.Email.Should().Be(userSignUpDTO.Email);
            userResponse.Roles.Should().Contain("User");

            accessToken.Should().Be("test_access_token");
            refreshToken.Should().NotBeNull();

            _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<ApplicationUser>(), password), Times.Once);
            _userManagerMock.Verify(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"), Times.Once);
            _jwtTokenGeneratorMock.Verify(t => t.GenerateJwtToken(It.IsAny<ApplicationUser>()), Times.Once);
        }

        [Fact]
        public async Task SignInUser_ValidInput_ReturnUserResponseDTO()
        {
            //Arrange
            var password = "TestPassword";
            var userSignUpDTO = new UserSignupDTO()
            {
                FirstName = "firstName",
                LastName = "lastName",
                UserName = "TestUserName",
                PhoneNumber = "TestPhoneNumber",
                Email = "TestEmail",
                Password = password
            };

            var userSigninDTO = new UserSigninDTO()
            {
                Email = "TestEmail",
                Password = password
            };

            var user = new ApplicationUser
            {
                UserName = userSignUpDTO.UserName,
                Email = userSignUpDTO.Email,
                FirstName = userSignUpDTO.FirstName,
                LastName = userSignUpDTO.LastName,
                PhoneNumber = userSignUpDTO.PhoneNumber,
                Id = "TestId"
            };
            _userManagerMock.Setup(u => u.FindByEmailAsync(userSignUpDTO.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.CheckPasswordAsync(It.IsAny<ApplicationUser>(), password)).ReturnsAsync(true);
            _jwtTokenGeneratorMock.Setup(t => t.GenerateJwtToken(It.IsAny<ApplicationUser>())).Returns("test_access_token");

            _mapperMock.Setup(m => m.Map<UserResponseDTO>(user)).Returns(new UserResponseDTO
            {
                UserName = userSignUpDTO.UserName,
                Email = userSignUpDTO.Email,
                Id = "TestId",
            });

            _userManagerMock.Setup(u => u.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(["User"]);

            //Act
            var result = await _authServiceMock.SigninAsync(userSigninDTO);

            //Assert
            result.Should().NotBeNull();
            var (userResponse, accessToken, refreshToken) = result;

            userResponse.Should().NotBeNull();
            userResponse.Should().BeOfType<UserResponseDTO>();
            userResponse.UserName.Should().Be(userSignUpDTO.UserName);
            userResponse.Email.Should().Be(userSignUpDTO.Email);
            userResponse.Roles.Should().Contain("User");

            accessToken.Should().Be("test_access_token");
            refreshToken.Should().NotBeNull();

            _userManagerMock.Verify(u => u.CheckPasswordAsync(It.IsAny<ApplicationUser>(), password), Times.Once);
            _jwtTokenGeneratorMock.Verify(t => t.GenerateJwtToken(It.IsAny<ApplicationUser>()), Times.Once);
            _userManagerMock.Verify(u => u.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Once);
        }

        [Fact]
        public async Task SigninUser_WrongCredentialInput_ReturnNoFoundException()
        {
            //Arrange
            var password = "TestPassword";
            var userSignUpDTO = new UserSignupDTO()
            {
                FirstName = "firstName",
                LastName = "lastName",
                UserName = "TestUserName",
                PhoneNumber = "TestPhoneNumber",
                Email = "TestEmail",
                Password = password
            };
            var userSignInDTO = new UserSigninDTO()
            {
                Email = "TestEmail",
                Password = password
            };
            var user = new ApplicationUser
            {
                UserName = userSignUpDTO.UserName,
                Email = userSignUpDTO.Email,
                FirstName = userSignUpDTO.FirstName,
                LastName = userSignUpDTO.LastName,
                PhoneNumber = userSignUpDTO.PhoneNumber,
                Id = "TestId"
            };
            _userManagerMock.Setup(u => u.FindByEmailAsync(userSignUpDTO.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(u => u.CheckPasswordAsync(user, password)).ReturnsAsync(false);

            //Act & Assert
            await _authServiceMock.Invoking(x => x.SigninAsync(userSignInDTO))
                .Should().ThrowAsync<NotFoundException>()
                .WithMessage("Email does not exist or incorrect password");
        }
    }
}