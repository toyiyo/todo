
using System.Linq;
using Abp.Domain.Services;
using Stripe;
using toyiyo.todo.Debugging;
using Abp.UI;
using System;

namespace toyiyo.todo.Core.Subscriptions
{
    public class SubscriptionManager : DomainService, ISubscriptionManager
    {
        public SubscriptionManager()
        {
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
                ReturnUrl= returnUrl,
            };
            var service = new Stripe.BillingPortal.SessionService();
            return service.Create(options);
        }
    }
}