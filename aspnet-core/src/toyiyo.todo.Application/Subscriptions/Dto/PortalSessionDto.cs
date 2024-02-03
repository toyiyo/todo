using System;
using Abp.AutoMapper;
using Stripe;

namespace toyiyo.todo.application.subscriptions
{
    /// <summary>
    /// Represents a data transfer object for a portal session.
    /// </summary>
    [AutoMap(typeof(Stripe.BillingPortal.Session))]
    public class PortalSessionDto
    {
        /// <summary>
        /// Gets or sets the ID of the portal session.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Portal session creation date.
        /// </summary>
        public DateTime Created { get; set; }
        /// <summary>
        /// The ID of the customer for this session.
        /// </summary>
        public string Customer { get; set; }
        /// <summary>
        /// Livemode determines if the session is in live mode or test mode.
        /// </summary>
        public bool Livemode { get; set; }
        /// <summary>
        /// The URL to which Stripe should send customers when they click on the link to return to your website.
        /// </summary>
        public string ReturnUrl { get; set; }
        /// <summary>
        /// The short-lived URL of the session giving customers access to the customer portal.
        /// </summary>
        public string Url { get; set; }
    }
}