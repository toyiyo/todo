using System.Threading.Tasks;
using Stripe;

namespace toyiyo.todo.application.subscriptions
{
    /// <summary>
    /// Represents a service for managing subscriptions.
    /// </summary>
    public interface ISubscriptionsService
    {
        Task<SubscriptionDto> Get(string id);
        Task<SubscriptionDto> GetSubscriptionForTenant();
        Task<ProductDto> GetProduct(string productId);
        Task<PortalSessionDto> CreatePortalSession(string stripeCustomerId, string returnUrl);
    }
}