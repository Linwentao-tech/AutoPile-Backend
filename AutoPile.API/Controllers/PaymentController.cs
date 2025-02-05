using AutoPile.DOMAIN.DTOs.Requests;
using AutoPile.DOMAIN.Models;
using AutoPile.SERVICE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoPile.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        /// <summary>
        /// Creates a new payment intent
        /// </summary>
        /// <param name="paymentIntentCreate">Payment intent creation details</param>
        /// <returns>Payment intent client secret</returns>
        [HttpPost("CreatePayment", Name = "CreatePayment")]
        [Authorize]
        public async Task<IActionResult> CreatePayment(PaymentIntentCreate paymentIntentCreate)
        {
            var intent = await _paymentService.PaymentIntentCreateAsync(paymentIntentCreate);
            return ApiResponse<object>.OkResult(new { client_secret = intent.ClientSecret });
        }
    }
}