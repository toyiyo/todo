using AutoMapper;
using Stripe;
using Stripe.BillingPortal;
namespace toyiyo.todo.application.subscriptions
{
    public class SubscriptionsMapProfile : Profile
    {
        public SubscriptionsMapProfile()
        {
            CreateMap<Subscription, SubscriptionDto>().ReverseMap();
            CreateMap<Session, PortalSessionDto>().ReverseMap();
            CreateMap<Product, ProductDto>().ReverseMap();
        }
    }
}