using Abp.AutoMapper;
using Stripe;

namespace toyiyo.todo.application.subscriptions
{
    /// <summary>
    /// Stripe's product instance representation, this is what you are selling.
    /// </summary>
    [AutoMap(typeof(Product))]
    public class ProductDto
    {
        /// <summary>
        /// Gets or sets the ID of the product.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Stripe's product name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Livemode determines if the session is in live mode or test mode.
        /// </summary>
        public string Livemode { get; set; }
        /// <summary>
        /// The products description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Is the product active
        /// </summary>
        public bool Active { get; set; }    
    }
}