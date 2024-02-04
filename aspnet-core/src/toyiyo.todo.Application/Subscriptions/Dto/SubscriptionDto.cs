using System;
using Abp.AutoMapper;
using Stripe;
namespace toyiyo.todo.application.subscriptions
{
    /// <summary>
    /// Represents a subscription for the tenant.
    /// </summary>
    /// <summary>
    /// Represents a data transfer object for a subscription.
    /// </summary>
    [AutoMap(typeof(Subscription))]
    public class SubscriptionDto
    {
        /// <summary>
        /// Gets or sets the ID of the subscription.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the subscription is active.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets the amount of the subscription.
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// Gets or sets the amount of the subscription in decimal format.
        /// </summary>
        public string AmountDecimal { get; set; }

        /// <summary>
        /// Gets or sets the billing scheme of the subscription.
        /// </summary>
        public string BillingScheme { get; set; }

        /// <summary>
        /// Gets or sets the creation date of the subscription.
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Gets or sets the interval count of the subscription.
        /// </summary>
        public int IntervalCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the subscription is in live mode.
        /// </summary>
        public bool LiveMode { get; set; }

        /// <summary>
        /// Gets or sets the nickname of the subscription.
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// Gets or sets the ID of the product associated with the subscription.
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the plan associated with the subscription.
        /// </summary>
        public string PlanId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the customer associated with the subscription.
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the URL of the portal session associated with the subscription.
        /// </summary>
        public string PortalSessionUrl { get; set; }

        /// <summary>
        /// Gets or sets the name of the product associated with the subscription.
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// Gets or sets the number of seats for the subscription.
        /// </summary>
        public int Seats { get; set; }
    }
}