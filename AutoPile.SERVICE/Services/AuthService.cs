using AutoMapper;
using AutoPile.DATA.Cache;
using AutoPile.DATA.Exceptions;
using AutoPile.DOMAIN.DTOs.Requests;
using AutoPile.DOMAIN.DTOs.Responses;
using AutoPile.DOMAIN.Models.Entities;
using AutoPile.DOMAIN.Models.MessageQueue;
using AutoPile.SERVICE.Utilities;
using DnsClient.Internal;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Resend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.SERVICE.Services
{
    public class AuthService : IAuthService
    {
        private const int TOKEN_EXPIRY_MINUTES = 15;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IResend _resend;
        private readonly IConfiguration _configuration;
        private readonly IEmailQueueService _emailQueueService;
        private readonly IUserInfoCache _userInfoCache;
        private readonly ILogger<IAuthService> _logger;

        public AuthService(UserManager<ApplicationUser> userManager, ILogger<IAuthService> logger, IEmailQueueService emailQueueService, IConfiguration configuration, IMapper mapper, IUserInfoCache userInfoCache, IJwtTokenGenerator jwtTokenGenerator, IResend resend)
        {
            _userManager = userManager;
            _mapper = mapper;
            _jwtTokenGenerator = jwtTokenGenerator;
            _resend = resend;
            _configuration = configuration;
            _emailQueueService = emailQueueService;
            _userInfoCache = userInfoCache;
            _logger = logger;
        }

        public async Task<(UserResponseDTO, string)> SignupAdminAsync(UserSignupDTO userSignupDTO)
        {
            var existingUser = await _userManager.FindByEmailAsync(userSignupDTO.Email);
            if (existingUser != null)
            {
                throw new BadRequestException("Email already registered");
            }

            var existUserWithPhone = await _userManager.Users.AnyAsync(u => u.PhoneNumber == userSignupDTO.PhoneNumber);
            if (existUserWithPhone)
            {
                throw new BadRequestException("Phone number already registered");
            }

            var user = new ApplicationUser
            {
                UserName = userSignupDTO.UserName,
                Email = userSignupDTO.Email,
                FirstName = userSignupDTO.FirstName,
                LastName = userSignupDTO.LastName,
                PhoneNumber = userSignupDTO.PhoneNumber,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, userSignupDTO.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                throw new BadRequestException($"Failed to create user: {string.Join(", ", errors)}");
            }

            var addToAdminResult = await _userManager.AddToRoleAsync(user, "Admin");
            if (!addToAdminResult.Succeeded)
            {
                var errors = addToAdminResult.Errors.Select(e => e.Description);
                throw new BadRequestException($"Failed to add user to role: {string.Join(", ", errors)}");
            }

            var addToUserResult = await _userManager.AddToRoleAsync(user, "User");
            if (!addToUserResult.Succeeded)
            {
                var errors = addToUserResult.Errors.Select(e => e.Description);
                throw new BadRequestException($"Failed to add user to role: {string.Join(", ", errors)}");
            }

            var token = _jwtTokenGenerator.GenerateJwtToken(user);

            var responseDTO = new UserResponseDTO
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,

                Roles = await _userManager.GetRolesAsync(user)
            };

            return (responseDTO, token);
        }

        public async Task<(UserResponseDTO, string)> SignupUserAsync(UserSignupDTO userSignupDTO)
        {
            var existingUser = await _userManager.FindByEmailAsync(userSignupDTO.Email);
            if (existingUser != null)
            {
                throw new BadRequestException("Email already registered");
            }

            var existUserWithPhone = await _userManager.Users.AnyAsync(u => u.PhoneNumber == userSignupDTO.PhoneNumber);
            if (existUserWithPhone)
            {
                throw new BadRequestException("Phone number already registered");
            }

            var user = new ApplicationUser
            {
                UserName = userSignupDTO.UserName,
                Email = userSignupDTO.Email,
                FirstName = userSignupDTO.FirstName,
                LastName = userSignupDTO.LastName,
                PhoneNumber = userSignupDTO.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, userSignupDTO.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                throw new BadRequestException($"Failed to create user: {string.Join(", ", errors)}");
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(user, "User");
            if (!addToRoleResult.Succeeded)
            {
                var errors = addToRoleResult.Errors.Select(e => e.Description);
                throw new BadRequestException($"Failed to add user to role: {string.Join(", ", errors)}");
            }

            var token = _jwtTokenGenerator.GenerateJwtToken(user);

            var responseDTO = new UserResponseDTO
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Roles = await _userManager.GetRolesAsync(user)
            };

            return (responseDTO, token);
        }

        public async Task<(UserResponseDTO, string)> SigninAsync(UserSigninDTO userSigninDTO)
        {
            var user = await _userManager.FindByEmailAsync(userSigninDTO.Email);

            if (user != null && await _userManager.CheckPasswordAsync(user, userSigninDTO.Password))
            {
                var token = _jwtTokenGenerator.GenerateJwtToken(user);
                UserResponseDTO userResponseDTO = _mapper.Map<UserResponseDTO>(user);

                userResponseDTO.Roles = await _userManager.GetRolesAsync(user);
                var userResponseInfoDTO = _mapper.Map<UserInfoResponseDTO>(user);
                userResponseInfoDTO.Roles = userResponseDTO.Roles;
                await _userInfoCache.SetUserAsync(user.Id, userResponseInfoDTO);
                _logger.LogInformation("Successfully cached user info for user {UserId}", user.Id);
                return (userResponseDTO, token);
            }

            throw new NotFoundException("Email does not exist or incorrect password");
        }

        public async Task<UserInfoResponseDTO> GetUserInfoAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new BadRequestException("user id is not found");
            }

            var userCached = await _userInfoCache.GetUserAsync(userId);
            if (userCached != null)
            {
                return userCached;
            }

            var user = await _userManager.FindByIdAsync(userId) ?? throw new NotFoundException($"User with ID {userId} not found");
            UserInfoResponseDTO userInfoResponseDTO = _mapper.Map<UserInfoResponseDTO>(user);
            userInfoResponseDTO.Roles = await _userManager.GetRolesAsync(user);
            return userInfoResponseDTO;
        }

        public async Task<string> SendEmailConfirmationTokenAsync(string email, string userId)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new BadRequestException("Email is required");
            }
            var user = await _userManager.FindByEmailAsync(email) ?? throw new NotFoundException($"User with email {email} not found");
            if (user.Id != userId)
            {
                throw new ForbiddenException();
            }
            var isConfirmed = await _userManager.IsEmailConfirmedAsync(user);
            if (isConfirmed)
            {
                throw new BadRequestException("This email is already confirmed");
            }
            if (user.EmailVerifyTokenCreatedAt.HasValue &&
        DateTime.UtcNow < user.EmailVerifyTokenCreatedAt.Value.AddMinutes(TOKEN_EXPIRY_MINUTES))
            {
                throw new BadRequestException($"A confirmation link has already been sent. Please wait {TOKEN_EXPIRY_MINUTES} minutes before requesting a new one.");
            }
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            user.EmailVerifyToken = token;
            user.EmailVerifyTokenCreatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var emailConfirmationUrl = $"{Environment.GetEnvironmentVariable("DOMAIN") ?? _configuration["Domain"]}/Auth/VerifyEmailConfirmationLink?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(email)}";
            //var message = new EmailMessage();
            //message.From = "Emailconfirm@autopile.store";
            //message.To.Add(email);
            //message.Subject = "Email Confirmation Link";
            //message.HtmlBody = $@"
            //    <p>Hello,</p>
            //    <p>Please click the link below to verify your email:</p>
            //    <p><a href='{emailConfirmationUrl}'>Email confirmation link</a></p>
            //    <p>If you did not sign up, please ignore this email.</p>"; ;

            //await _resend.EmailSendAsync(message);
            var emailMessage = new DOMAIN.Models.MessageQueue.EmailMessage
            {
                To = email,
                Subject = "Email Confirmation Link",
                MessageType = "EmailConfirmation",
                Body = $@"<p>Hello,</p><p>Please click the link below to verify your email:</p>
                     <p><a href='{emailConfirmationUrl}'>Email confirmation link</a></p>",
                AdditionalData = new Dictionary<string, string>
                {
                    ["UserId"] = userId,
                    ["Token"] = token
                }
            };

            await _emailQueueService.QueueEmailMessage(emailMessage);
            return token.ToString();
        }

        public async Task<bool> VerifyEmailConfirmationTokenAsync(string token, string email)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                return false;
            }
            var user = await _userManager.FindByEmailAsync(email) ?? throw new NotFoundException($"User with email {email} not found");

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                return false;
            }
            if (user.EmailVerifyToken != token)
            {
                return false;
            }

            if (!user.EmailVerifyTokenCreatedAt.HasValue ||
        DateTime.UtcNow > user.EmailVerifyTokenCreatedAt.Value.AddMinutes(TOKEN_EXPIRY_MINUTES))
            {
                return false;
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                user.EmailVerifyToken = null;
                user.EmailVerifyTokenCreatedAt = null;
                await _userManager.UpdateAsync(user);
            }

            return result.Succeeded;
        }

        public async Task UpdateUserInfoAsync(UserUpdateInfoDTO userUpdateInfoDTO, string userId)
        {
            if (userId == null)
            {
                throw new NotFoundException("User ID not found in token.");
            }

            var user = await _userManager.FindByIdAsync(userId) ?? throw new NotFoundException("User not found");
            _mapper.Map(userUpdateInfoDTO, user);

            await _userInfoCache.SetUserAsync(userId, _mapper.Map<UserInfoResponseDTO>(user));
            await _userManager.UpdateAsync(user);
        }

        public async Task<string> SendResetPasswordTokenAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new BadRequestException("email is required");
            }
            var user = await _userManager.FindByEmailAsync(email) ?? throw new NotFoundException($"User with email {email} not found");
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var emailResetUrl = $"https://www.autopile.store/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(email)}";
            //var message = new Resend.EmailMessage();
            //message.From = "PasswordReset@autopile.store";
            //message.To.Add(email);
            //message.Subject = "Password Reset Link";
            //message.HtmlBody = $@"
            //    <p>Hello,</p>
            //    <p>Please click the link below to Reset your password:</p>
            //    <p><a href='{emailConfirmationUrl}'>Password Reset link</a></p>
            //    <p>If you did not request password reset, please ignore this email.</p>"; ;

            //await _resend.EmailSendAsync(message);

            var emailMessage = new DOMAIN.Models.MessageQueue.EmailMessage
            {
                To = email,
                Subject = "Password Reset Link",
                MessageType = "PasswordReset",
                Body = $@"<p>Hello,</p><p>Please click the link below to Reset your password:</p>
                     <p><a href='{emailResetUrl}'>Password Reset link</a></p>
                     <p>If you did not request password reset, please ignore this email.</p>"
            };

            await _emailQueueService.QueueEmailMessage(emailMessage);
            return token.ToString();
        }

        public async Task ResetPasswordAsync(UserResetPasswordDTO userResetPasswordDTO)
        {
            var user = await _userManager.FindByEmailAsync(userResetPasswordDTO.Email) ?? throw new NotFoundException($"User with email {userResetPasswordDTO.Email} not found");

            var result = await _userManager.ResetPasswordAsync(user, userResetPasswordDTO.EmailVerifyToken, userResetPasswordDTO.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BadRequestException($"Password reset failed: {errors}");
            }
        }

        public async Task ValidatePasswordResetTokenAsync(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                throw new BadRequestException("Email and token are required");
            }

            var user = await _userManager.FindByEmailAsync(email)
                ?? throw new NotFoundException($"User with email {email} not found");

            bool isValid = await _userManager.VerifyUserTokenAsync(
                user,
                _userManager.Options.Tokens.PasswordResetTokenProvider,
                UserManager<IdentityUser>.ResetPasswordTokenPurpose,
                token
            );
            if (!isValid)
            {
                throw new BadRequestException("Token expired or invalid");
            }
        }
    }
}