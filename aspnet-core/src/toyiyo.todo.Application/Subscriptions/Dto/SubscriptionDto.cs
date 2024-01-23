using System;
using Abp.AutoMapper;
using Stripe;
namespace toyiyo.todo.application.subscriptions
{
    [AutoMap(typeof(Subscription))]
    public class SubscriptionDto
    {
        public string Id { get; set; }
        public bool Active { get; set; }
        public int Amount { get; set; }
        public string AmountDecimal { get; set; }
        public string BillingScheme { get; set; }
        public DateTime Created { get; set; }
        public int IntervalCount { get; set; }
        public bool LiveMode { get; set; }
        public string Nickname { get; set; }
        public string ProductId { get; set; }
        public string PlanId { get; set; }
        public string CustomerId {get; set;}
        public string PortalSessionUrl { get; set; }
        public string ProductName { get; set; }
    }
}