using MicroCommerce.ApiService.Infrastructure;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Tax;

namespace MicroCommerce.ApiService.Endpoints;

public static class PaymentEndpoint
{
    public static void MapPayments(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("api/payments")
            .WithTags("payments");

        group.MapGet("/config", (IOptions<StripeOptions> options) => new { options.Value.PublishableKey });

        group.MapGet("/create-payment-intent", async (IConfiguration configuration) =>
        {
            var calcuateTax = configuration.GetSection("Stripe").GetValue<bool>("CalculateTax");

            try
            {
                long orderAmount = 1400;

                var service = new PaymentIntentService();
                PaymentIntent paymentIntent = default;

                if (calcuateTax)
                {
                    paymentIntent = await service.CreateAsync(new()
                    {
                        Amount = orderAmount,
                        Currency = "USD",
                        AutomaticPaymentMethods = new() { Enabled = true }
                    });
                }
                else
                {
                    paymentIntent = await service.CreateAsync(new()
                    {
                        Amount = orderAmount,
                        Currency = "USD",
                        AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true }
                    });
                }

                return Results.Ok(new { paymentIntent.ClientSecret });
            }
            catch (StripeException e)
            {
                return Results.BadRequest(new { error = new { message = e.StripeError.Message } });
            }
        });


        group.MapPost("/webhook", async (HttpRequest request, IOptions<StripeOptions> options, ILogger logger) =>
        {
            var json = await new StreamReader(request.Body).ReadToEndAsync();
            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    request.Headers["Stripe-Signature"],
                    options.Value.WebhookSecret
                );
                logger.LogInformation($"Webhook notification with type: {stripeEvent.Type} found for {stripeEvent.Id}");
            }
            catch (Exception e)
            {
                logger.LogInformation($"Something failed {e}");
                return Results.BadRequest();
            }

            if (stripeEvent.Type == Events.PaymentIntentSucceeded)
            {
                var paymentIntent = stripeEvent.Data.Object as Stripe.PaymentIntent;
                logger.LogInformation($"PaymentIntent ID: {paymentIntent.Id}");
                // Take some action based on the payment intent.
            }

            return Results.Ok();
        });
    }
}