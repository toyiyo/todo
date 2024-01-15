
using System.Linq;
using Abp.Domain.Services;
using Stripe;
using toyiyo.todo.Debugging;
using Abp.UI;
using System;

namespace toyiyo.todo.Core.Subscriptions
{
    public class SubscriptionManager : DomainService
    {
        //private readonly IStripeClient _stripeClient;

        public SubscriptionManager()
        {
            LocalizationSourceName = todoConsts.LocalizationSourceName;
            //_stripeClient = stripeClient;
            //todo: move keys to environment variables
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
                //todo: check if there are more than one customer with the same email, this should not happen
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
    }
}