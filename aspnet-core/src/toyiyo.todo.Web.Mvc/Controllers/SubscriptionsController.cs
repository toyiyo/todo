using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.Mvc;
using toyiyo.todo.application.subscriptions;
using toyiyo.todo.Authorization;
using toyiyo.todo.Authorization.Users;
using toyiyo.todo.Controllers;
using toyiyo.todo.Users;

namespace toyiyo.todo.Web.Controllers
{
    [AbpMvcAuthorize(PermissionNames.Pages_Subscription)]
    public class SubscriptionsController : todoControllerBase
    {
        private readonly ISubscriptionsService _subscriptionsService;
        private readonly UserManager _userManager;

        public SubscriptionsController(ISubscriptionsService subscriptionsService, UserManager userManager)
        {
            _subscriptionsService = subscriptionsService;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            // Get the subscription for the current tenant
            var subscriptionDto = await _subscriptionsService.GetSubscriptionForTenant();

            //get the current
            var user = await _userManager.FindByIdAsync(AbpSession.GetUserId().ToString());
            //pass email and userid to view
            ViewBag.Email = user.EmailAddress;
            ViewBag.TenantId = AbpSession.GetTenantId();
            ViewBag.Seats = _userManager.Users.Count();

            // Check if subscriptionDto is not null and has a valid ProductId and PlanId
            if (subscriptionDto != null && !string.IsNullOrEmpty(subscriptionDto.ProductId) && !string.IsNullOrEmpty(subscriptionDto.PlanId))
            {
                var productDto = _subscriptionsService.GetProduct(subscriptionDto.ProductId);

                string returnUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}";
                //we have an existing subscription, let's create a management session and set the URL so the user can manage their subscription
                var portalSession = _subscriptionsService.CreatePortalSession(subscriptionDto.CustomerId, returnUrl);
                subscriptionDto.PortalSessionUrl = portalSession.Url;
                subscriptionDto.ProductName = productDto.Name;
            }

            return View(subscriptionDto);
        }
    }
}