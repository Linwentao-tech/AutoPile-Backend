using AutoPile.DOMAIN.DTOs.Requests;
using Stripe;

namespace AutoPile.SERVICE.Services
{
    public interface IPaymentService
    {
        Task<PaymentIntent> PaymentIntentCreateAsync(PaymentIntentCreate paymentIntentCreate);
    }
}