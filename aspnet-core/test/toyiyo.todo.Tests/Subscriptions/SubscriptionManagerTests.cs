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
        private readonly IStripeClient _stripeClient;

        public SubscriptoinManagerTests()
        {
            _subscriptionManager = Resolve<SubscriptionManager>();
        }

        [Fact]
        public async Task GetSubscriptionCustomerByEmail_ValidEmail_ReturnsCustomer()
        {
            // Arrange
            var email = "jdelgado@toyiyo.com";

            // Act
            var result = _subscriptionManager.GetSubscriptionCustomerByEmail(email);

            // Assert
            result.ShouldNotBeNull();
            result.Email.ShouldBe(email);
        }

        [Fact]
        public async Task GetSubscriptionCustomerByEmail_InvalidEmail_ThrowsException()
        {
            // Arrange
            var email = "invalid@example.com";

            // Act & Assert
            Should.Throw<UserFriendlyException>(() => _subscriptionManager.GetSubscriptionCustomerByEmail(email));
        }

        [Fact]
        public async Task GetSubscriptionByEmail_ValidEmail_ReturnsSubscription()
        {
            // Arrange
            var email = "jdelgado@toyiyo.com";

            // Act
            var result = _subscriptionManager.GetSubscriptionByEmail(email);

            // Assert
            result.ShouldNotBeNull();
            result.Data.ShouldNotBeNull();
            result.Data.First().Status.ShouldBe("active");
        }

        [Fact]
        public async Task GetSubscriptionByEmail_InvalidEmail_ThrowsException()
        {
            // Arrange
            var email = "invalid@example.com";

            // Act & Assert
            Should.Throw<UserFriendlyException>(() => _subscriptionManager.GetSubscriptionByEmail(email));
        }

    }
}