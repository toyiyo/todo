using System.Threading.Tasks;
using Stripe;

namespace toyiyo.todo.application.subscriptions
{
    /// <summary>
    /// Represents a service for managing subscriptions.
    /// </summary>
    /// <summary>
    /// Represents a service for managing subscriptions.
    /// </summary>
    public interface ISubscriptionsService
    {
        /// <summary>
        /// Retrieves a subscription by its ID.
        /// </summary>
        /// <param name="id">The ID of the subscription.</param>
        /// <returns>The subscription DTO.</returns>
        SubscriptionDto Get(string id);

        /// <summary>
        /// Retrieves the subscription for the current tenant.
        /// </summary>
        /// <returns>The subscription DTO.</returns>
        Task<SubscriptionDto> GetSubscriptionForTenant();

        /// <summary>
        /// Retrieves a product by its ID.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <returns>The product DTO.</returns>
        ProductDto GetProduct(string productId);

        /// <summary>
        /// Creates a portal session for the specified Stripe customer and return URL.
        /// </summary>
        /// <param name="stripeCustomerId">The ID of the Stripe customer.</param>
        /// <param name="returnUrl">The return URL for the portal session.</param>
        /// <returns>The portal session DTO.</returns>
        PortalSessionDto CreatePortalSession(string stripeCustomerId, string returnUrl);
    }
}