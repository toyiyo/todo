
using System.Linq;
using Abp.Domain.Services;
using Stripe;
using toyiyo.todo.Debugging;
using Abp.UI;
using System;
using Stripe.Checkout;
using toyiyo.todo.Authorization.Users;
using System.Threading.Tasks;
using toyiyo.todo.MultiTenancy;
using Abp.Domain.Uow;
using System.Configuration;

namespace toyiyo.todo.Core.Subscriptions
{
    public class SubscriptionManager : DomainService, ISubscriptionManager
    {
        private readonly TenantManager _tenantManager;
        private readonly UserManager _userManager;

        private readonly string _webhookSecret;
        public SubscriptionManager(TenantManager tenantManager, UserManager userManager)
        {
            _tenantManager = tenantManager;
            _userManager = userManager;
            LocalizationSourceName = todoConsts.LocalizationSourceName;
            _webhookSecret = Environment.GetEnvironmentVariable("StripeWebhookSecret") ?? throw new ConfigurationErrorsException("StripeWebhookSecret environment variable is not set.");
            StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("StripeAPIKeyProduction") ?? throw new ConfigurationErrorsException("StripeAPIKeyProduction environment variable is not set.");
        }

        public Subscription GetSubscriptionByTenantId(int tenantId)
        {
            var tenant = _tenantManager.GetById(tenantId);
            return GetSubscriptionById(tenant.ExternalSubscriptionId);
        }

        public Subscription GetSubscriptionById(string subscriptionId)
        {
            try
            {
                var service = new SubscriptionService();
                return service.Get(subscriptionId);
            }
            catch (System.Exception)
            {
                return new Subscription();
            }
        }

        public Product GetProduct(string productId)
        {
            try
            {
                var service = new ProductService();
                return service.Get(productId);
            }
            catch (System.Exception)
            {
                return new Product();
            }
        }

        public Stripe.BillingPortal.Session CreateBillingPortalConfiguration(string stripeCustomerId, string returnUrl)
        {
            var options = new Stripe.BillingPortal.SessionCreateOptions
            {
                Customer = stripeCustomerId,
                ReturnUrl = returnUrl,
            };
            var service = new Stripe.BillingPortal.SessionService();
            return service.Create(options);
        }

        public async Task StripeWebhookHandler(string json, string stripeSignatureHeader)
        {
            if (string.IsNullOrEmpty(stripeSignatureHeader)) throw new ArgumentException("stripeSignatureHeader cannot be null or empty", nameof(stripeSignatureHeader));

            var stripeEvent = EventUtility.ConstructEvent(
              json,
              stripeSignatureHeader,
              _webhookSecret
            );

            // Handle the checkout.session.completed event
            if (stripeEvent.Type == Events.CheckoutSessionCompleted)
            {
                await CheckoutSessionCompletedHandler(stripeEvent);
            }
            if (stripeEvent.Type == Events.CustomerSubscriptionUpdated)
            {
                await CustomerSubscriptionUpdatedHandler(stripeEvent);
            }
        }
        /// <summary>
        /// When a customer's subscription is updated, we find the tenant by the stripe subscription id and update the tenant's seat count
        /// We default to 0 if the quantity is not set or there is an error retrieving the quantity
        /// </summary>
        /// <param name="stripeEvent"></param>
        /// <returns></returns>
        private async Task CustomerSubscriptionUpdatedHandler(Event stripeEvent)
        {
            var subscription = stripeEvent.Data.Object as Subscription;
            var tenant = await _tenantManager.GetByExternalSubscriptionIdAsync(subscription.Id);
            if (tenant != null)
            {
                using (CurrentUnitOfWork.SetTenantId(tenant.Id))
                {
                    var quantity = subscription.Items.Data?.FirstOrDefault()?.Quantity ?? 0;
                    await _tenantManager.SetSubscriptionSeats(tenant, (int)quantity);

                    await MatchActiveUsersToSeatsAvailable(tenant, quantity);
                }
            }
            LogStripeEventProcessing(stripeEvent.Type);
        }

        private async Task MatchActiveUsersToSeatsAvailable(Tenant tenant, long quantity)
        {
            var tenantUsers = _userManager.Users.Where(u => u.TenantId == tenant.Id).ToList();
            var activeUsers = tenantUsers.Where(u => u.IsActive).ToList();

            if (activeUsers.Count > quantity)
            {
                // If the number of active users is more than the new quantity, deactivate the recently created users
                var usersToDeactivate = activeUsers.OrderByDescending(u => u.CreationTime).Take((int)(activeUsers.Count - quantity));
                foreach (var user in usersToDeactivate)
                {
                    user.IsActive = false;
                    await _userManager.UpdateAsync(user);
                }
            }
            else if (activeUsers.Count < quantity)
            {
                // If the number of active users is less than the new quantity, activate the previously inactive users
                var inactiveUsers = tenantUsers.Except(activeUsers).ToList();
                var usersToActivate = inactiveUsers.OrderBy(u => u.CreationTime).Take((int)(quantity - activeUsers.Count));
                foreach (var user in usersToActivate)
                {
                    user.IsActive = true;
                    await _userManager.UpdateAsync(user);
                }
            }
        }

        private async Task CheckoutSessionCompletedHandler(Event stripeEvent)
        {
            var session = stripeEvent.Data.Object as Session;
            var options = new SessionGetOptions();
            options.AddExpand("line_items");

            var service = new SessionService();
            // Retrieve the session. If you require line items in the response, you may include them by expanding line_items.
            Session sessionWithLineItems = service.Get(session.Id, options);

            // Fulfill the purchase...
            await FulfillOrderAsync(sessionWithLineItems);
            LogStripeEventProcessing(stripeEvent.Type);
        }

        private async Task FulfillOrderAsync(Session sessionWithLineItems)
        {
            //Saving a copy of the order in your own database.
            var tenant = await _tenantManager.GetByIdAsync(int.Parse(sessionWithLineItems.ClientReferenceId));
            using (CurrentUnitOfWork.SetTenantId(tenant.Id))
            {
                await _tenantManager.SetExternalSubscriptionId(tenant, sessionWithLineItems.SubscriptionId);
                await _tenantManager.SetSubscriptionSeats(tenant, (int)(sessionWithLineItems?.LineItems?.Data?.FirstOrDefault()?.Quantity ?? 1));
            }

            //update the tenant's seat (users) information

            //Sending the customer a receipt email.
            //Reconciling the line items and quantity purchased by the customer if using line_item.adjustable_quantity. 
            //If the Checkout Session has many line items you can paginate through them with the line_items.
        }

        private void LogStripeEventProcessing(string eventType)
        {
            Logger.Info($"Processed Stripe event: {eventType}");
        }
    }
}