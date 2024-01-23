using Abp.AutoMapper;
using Stripe;
namespace toyiyo.todo.application.subscriptions
{
    [AutoMap(typeof(Plan))]
    public class PlanDto
    {
    }
}