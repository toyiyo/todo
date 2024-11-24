using System;
using toyiyo.todo.Authorization.Users;
using Abp.Timing;
using Shouldly;
using Xunit;

namespace toyiyo.todo.Tests.Users
{
    public class UserInvitationTests
    {
        [Fact]
        public void Should_Create_Valid_Invitation()
        {
            // Arrange
            var tenantId = 1;
            var email = "test@example.com";
            var invitedByUserId = 1;
            var user = new User { Id = invitedByUserId };

            // Act
            var invitation = UserInvitation.CreateDefaultInvitation(tenantId, email, user);

            // Assert
            invitation.ShouldNotBeNull();
            invitation.TenantId.ShouldBe(tenantId);
            invitation.Email.ShouldBe(email);
            invitation.InvitedByUserId.ShouldBe(invitedByUserId);
            invitation.ExpirationDate.ShouldBeGreaterThan(Clock.Now);
            invitation.Token.ShouldNotBeNullOrEmpty();
            invitation.Status.ShouldBe(InvitationStatus.Pending);
            invitation.IsValid().ShouldBeTrue();
        }

        [Fact]
        public void Should_Throw_Exception_For_Invalid_Email()
        {
            // Arrange
            var tenantId = 1;
            var email = "invalid-email";
            var user = new User { Id = 1 };

            // Act & Assert
            Should.Throw<ArgumentException>(() =>
                UserInvitation.CreateDefaultInvitation(tenantId, email, user));
        }

        [Fact]
        public void Should_Validate_Token()
        {
            // Arrange
            var tenantId = 1;
            var email = "test@example.com";
            var user = new User { Id = 1 };
            var invitation = UserInvitation.CreateDefaultInvitation(tenantId, email, user);

            // Act
            var isValid = invitation.ValidateToken(invitation.Token);

            // Assert
            isValid.ShouldBeTrue();
        }

        [Fact]
        public void Should_Accept_Invitation()
        {
            // Arrange
            var tenantId = 1;
            var email = "test@example.com";
            var user = new User { Id = 1 };
            var invitation = UserInvitation.CreateDefaultInvitation(tenantId, email, user);
            var acceptedBy = new User { Id = 2, EmailAddress = email };

            // Act
            invitation.Accept(acceptedBy);

            // Assert
            invitation.AcceptedDate.ShouldNotBeNull();
            invitation.Status.ShouldBe(InvitationStatus.Accepted);
            invitation.IsValid().ShouldBeFalse();
        }

        [Fact]
        public void Should_Throw_Exception_For_Invalid_Acceptance()
        {
            // Arrange
            var tenantId = 1;
            var email = "test@example.com";
            var user = new User { Id = 1 };
            var invitation = UserInvitation.CreateDefaultInvitation(tenantId, email, user);
            var acceptedBy = new User { Id = 2, EmailAddress = "different@example.com" };

            // Act & Assert
            Should.Throw<InvalidOperationException>(() => invitation.Accept(acceptedBy));
        }
    }
}