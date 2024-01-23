using Abp.AutoMapper;
using Stripe;

namespace toyiyo.todo.application.subscriptions
{
    [AutoMap(typeof(Product))]
    public class ProductDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Livemode { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }    
    }
}