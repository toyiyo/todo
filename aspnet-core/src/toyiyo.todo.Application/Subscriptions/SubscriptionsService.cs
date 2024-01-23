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
        public async Task<SubscriptionDto> Get(string id)
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
            //todo: use tenant.subscriptionId to get the subscription - this will be saved when handling the webhook for a new subscription
            return await Get(tenant.ExternalSubscriptionId);
        }

        public async Task<PlanDto> GetPlan(string planId)
        {
            var plan = _subscriptionManager.GetPlan(planId);
            return ObjectMapper.Map<PlanDto>(plan);
        }

        public async Task<ProductDto> GetProduct(string productId)
        {
            var product = _subscriptionManager.GetProduct(productId);
            return ObjectMapper.Map<ProductDto>(product);
        }

        public async Task<PortalSessionDto> CreatePortalSession(string userId, string returnUrl)
        {
            var session = _subscriptionManager.CreateBillingPortalConfiguration(userId, returnUrl);
            return ObjectMapper.Map<PortalSessionDto>(session);
        }
    }
}