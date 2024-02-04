
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

namespace toyiyo.todo.Core.Subscriptions
{
    public class SubscriptionManager : DomainService, ISubscriptionManager
    {
        private readonly TenantManager _tenantManager;

        private readonly string webhookSecret = Environment.GetEnvironmentVariable("StripeWebhookSecret");
        public SubscriptionManager(TenantManager tenantManager)
        {
            _tenantManager = tenantManager;
            LocalizationSourceName = todoConsts.LocalizationSourceName;

            if (DebugHelper.IsDebug)
                StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("StripeAPIKeyDebug");
            else
                StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("StripeAPIKeyProduction");
        }

        public Customer GetSubscriptionCustomerByEmail(string email)
        {
            try
            {
                var options = new CustomerListOptions
                {
                    Email = email
                };
                var service = new CustomerService();
                StripeList<Customer> customers = service.List(options);

                if (!customers.Data.Any())
                {
                    throw new UserFriendlyException(L("GetSubscriptionCustomerByEmailNotFound", email));
                }

                return customers.Data.FirstOrDefault();
            }
            catch (StripeException e)
            {
                throw new UserFriendlyException(L("StripeApiError", e.StripeError.Code));
            }
            catch (System.Exception e)
            {
                throw new UserFriendlyException(L("GetSubscriptionCustomerByEmailError", e.Message));
            }
        }
        public StripeList<Subscription> GetSubscriptionByEmail(string email)
        {
            try
            {
                Customer customer = GetSubscriptionCustomerByEmail(email) ?? throw new UserFriendlyException(L("GetSubscriptionCustomerByEmailNotFound", email));

                var options = new SubscriptionListOptions
                {
                    Customer = customer.Id,
                    Status = "active",
                };

                var service = new SubscriptionService();

                StripeList<Subscription> subscriptions = service.List(options);

                if (!subscriptions.Data.Any())
                {
                    throw new UserFriendlyException(L("SubscriptionsNotFoundError", email));
                }

                return subscriptions;
            }
            catch (StripeException e)
            {
                throw new UserFriendlyException(L("StripeApiError", e.StripeError.Code));
            }
            catch (System.Exception e)
            {
                throw new UserFriendlyException(L("GetSubscriptionByEmailError", e.Message));
            }
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
            var stripeEvent = EventUtility.ConstructEvent(
              json,
              stripeSignatureHeader,
              webhookSecret
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
        }

        private async Task FulfillOrderAsync(Session sessionWithLineItems)
        {
            //Saving a copy of the order in your own database.
            var tenant = await _tenantManager.GetByIdAsync(int.Parse(sessionWithLineItems.ClientReferenceId));
            using (CurrentUnitOfWork.SetTenantId(tenant.Id)){ 
                await _tenantManager.SetExternalSubscriptionId(tenant, sessionWithLineItems.SubscriptionId);
                await _tenantManager.SetSubscriptionSeats(tenant, (int)(sessionWithLineItems?.LineItems?.Data?.FirstOrDefault()?.Quantity ?? 1));
            }
            
            //update the tenant's seat (users) information
        
            //Sending the customer a receipt email.
            //Reconciling the line items and quantity purchased by the customer if using line_item.adjustable_quantity. 
            //If the Checkout Session has many line items you can paginate through them with the line_items.
        }
    }
}