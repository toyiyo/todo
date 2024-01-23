using System;
using Abp.AutoMapper;
using Stripe;

namespace toyiyo.todo.application.subscriptions
{
    [AutoMap(typeof(Stripe.BillingPortal.Session))]
    public class PortalSessionDto
    {
        public string Id { get; set; }
        public string Object { get; set; }
        public DateTime Created { get; set; }
        public string Customer { get; set; }
        public bool Livemode { get; set; }
        public string Locale { get; set; }
        public string OnBehalfOf { get; set; }
        public string ReturnUrl { get; set; }
        public string Url { get; set; }
    }
}