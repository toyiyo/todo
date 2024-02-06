using System.Threading.Tasks;
using toyiyo.todo.Core.Subscriptions;
using Xunit;
using Shouldly;
using System.Text.Json;
using Abp.UI;
using Stripe;
using System;
using System.Linq;

namespace toyiyo.todo.Tests.Subscriptions
{
    public class SubscriptoinManagerTests : todoTestBase
    {
        private readonly SubscriptionManager _subscriptionManager;

        public SubscriptoinManagerTests()
        {
            _subscriptionManager = Resolve<SubscriptionManager>();
        }

    }
}