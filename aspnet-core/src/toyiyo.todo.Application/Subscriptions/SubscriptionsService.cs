using toyiyo.todo.Core.Subscriptions;
using System;
using System.Threading.Tasks;
using Stripe;
using System.Linq;

namespace toyiyo.todo.application.subscriptions
{
    /// <summary>
    /// Represents a service for managing subscriptions.
    /// </summary>
    public class SubscriptionsService : todoAppServiceBase, ISubscriptionsService
    {
        private readonly ISubscriptionManager _subscriptionManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionsService"/> class.
        /// </summary>
        /// <param name="subscriptionManager">The subscription manager.</param>
        public SubscriptionsService(ISubscriptionManager subscriptionManager)
        {
            _subscriptionManager = subscriptionManager;
        }

        /// <summary>
        /// Gets a Subscription by Id.
        /// </summary>
        /// <param name="id">The Id of the subscription.</param>
        /// <returns>The subscription.</returns>
        public SubscriptionDto Get(string id)
        {
            var subscription = _subscriptionManager.GetSubscriptionById(id);
            var Dto = ObjectMapper.Map<SubscriptionDto>(subscription);
            var firstItem = subscription?.Items?.Data?.FirstOrDefault();
            if (firstItem?.Plan != null)
            {
                Dto.ProductId = firstItem.Plan.ProductId;
                Dto.PlanId = firstItem.Plan.Id;
            }
            return Dto;
        }

        /// <summary>
        /// Gets the subscription for the current tenant.
        /// </summary>
        /// <returns>The subscription</returns>
        public async Task<SubscriptionDto> GetSubscriptionForTenant()
        {
            var tenant = await GetCurrentTenantAsync();
            
            return Get(tenant.ExternalSubscriptionId);
        }

        /// <summary>
        /// Retrieves a product by its ID.
        /// </summary>
        /// <param name="productId">The ID of the product to retrieve.</param>
        /// <returns>The product DTO.</returns>
        public ProductDto GetProduct(string productId)
        {
            var product = _subscriptionManager.GetProduct(productId);
            return ObjectMapper.Map<ProductDto>(product);
        }

        /// <summary>
        /// Creates a portal session for the specified user and return URL.
        /// </summary>
        /// <param name="stripeCustomerId">The ID of the stripe user.</param>
        /// <param name="returnUrl">The return URL after the portal session is created.</param>
        /// <returns>The portal session DTO.</returns>
        public PortalSessionDto CreatePortalSession(string stripeCustomerId, string returnUrl)
        {
            var session = _subscriptionManager.CreateBillingPortalConfiguration(stripeCustomerId, returnUrl);
            return ObjectMapper.Map<PortalSessionDto>(session);
        }
    }
}