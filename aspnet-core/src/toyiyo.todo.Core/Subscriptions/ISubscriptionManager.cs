using Stripe;

namespace toyiyo.todo.Core.Subscriptions
{
    public interface ISubscriptionManager
    {
        Customer GetSubscriptionCustomerByEmail(string email);
        StripeList<Subscription> GetSubscriptionByEmail(string email);
        Subscription GetSubscriptionById(string subscriptionId);
        Plan GetPlan(string planId);
        Product GetProduct(string productId);
        Stripe.BillingPortal.Session CreateBillingPortalConfiguration(string stripeCustomerId, string returnUrl);

    }
}