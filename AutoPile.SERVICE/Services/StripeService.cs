using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.SERVICE.Services
{
    public class StripeService : IStripeService
    {
        public async Task<PaymentIntent> CreatePaymentIntentAsync(long amount, string currency)
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = amount,
                Currency = currency,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
            };
            var service = new PaymentIntentService();
            return await service.CreateAsync(options);
        }
    }
}