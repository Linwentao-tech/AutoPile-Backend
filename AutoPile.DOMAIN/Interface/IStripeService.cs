using Stripe;

namespace AutoPile.SERVICE.Services
{
    public interface IStripeService
    {
        Task<PaymentIntent> CreatePaymentIntentAsync(long amount, string currency);
    }
}