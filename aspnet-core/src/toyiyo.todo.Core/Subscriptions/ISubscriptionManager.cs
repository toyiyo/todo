using System.Threading.Tasks;
using Stripe;

namespace toyiyo.todo.Core.Subscriptions
{
    public interface ISubscriptionManager
    {
        Subscription GetSubscriptionByTenantId(int tenantId);
        Subscription GetSubscriptionById(string subscriptionId);
        Product GetProduct(string productId);
        Stripe.BillingPortal.Session CreateBillingPortalConfiguration(string stripeCustomerId, string returnUrl);
        Task StripeWebhookHandler(string json, string stripeSignatureHeader);
    }
}