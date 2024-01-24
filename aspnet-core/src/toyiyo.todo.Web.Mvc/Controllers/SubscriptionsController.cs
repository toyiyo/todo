using System;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using toyiyo.todo.application.subscriptions;
using toyiyo.todo.Controllers;

namespace toyiyo.todo.Web.Controllers
{
    [AbpMvcAuthorize]
    public class SubscriptionsController : todoControllerBase
    {
        private readonly ISubscriptionsService _subscriptionsService;

        public SubscriptionsController(ISubscriptionsService subscriptionsService)
        {
            _subscriptionsService = subscriptionsService;
        }
        public async Task<IActionResult> Index()
        {
            var subscriptionDto = await _subscriptionsService.GetSubscriptionForTenant();

            // Check if subscriptionDto is not null and has a valid ProductId and PlanId
            if (subscriptionDto != null && !string.IsNullOrEmpty(subscriptionDto.ProductId) && !string.IsNullOrEmpty(subscriptionDto.PlanId))
            {
                var productDto = await _subscriptionsService.GetProduct(subscriptionDto.ProductId);

                Console.WriteLine($"Product: {productDto.Name}");

                string returnUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}";
                //we have an existing subscription, let's create a management session and set the URL so the user can manage their subscription
                var portalSession = await _subscriptionsService.CreatePortalSession(subscriptionDto.CustomerId, returnUrl);
                subscriptionDto.PortalSessionUrl = portalSession.Url;
                subscriptionDto.ProductName = productDto.Name;
            }
            return View(subscriptionDto);
        }
    }
}