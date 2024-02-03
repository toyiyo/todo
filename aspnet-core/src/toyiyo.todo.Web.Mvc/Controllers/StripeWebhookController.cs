using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using toyiyo.todo.Controllers;
using toyiyo.todo.Core.Subscriptions;

namespace workspace.Controllers
{

    [Route("api/[controller]")]
    public class StripeWebHook : todoControllerBase
    {
        private readonly ISubscriptionManager _subscriptionManager;
        public StripeWebHook(ISubscriptionManager subscriptionManager)
        {
            _subscriptionManager = subscriptionManager;
        }

        [HttpPost]
        public async Task<IActionResult> Index()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                await _subscriptionManager.StripeWebhookHandler(json, GetStripeSignatureHeader());

                return Ok();
            }
            catch (StripeException e)
            {
                return BadRequest(e.Message);
            }
        }
        private string GetStripeSignatureHeader()
        {
            return Request.Headers["Stripe-Signature"];
        }
    }
}
