using System;
using toyiyo.todo.Authorization.Users;
using Abp.Timing;
using Shouldly;
using Xunit;
using toyiyo.todo.Invitations;

namespace toyiyo.todo.Tests.Invitations
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
        [Fact]
        public void Accept_Should_Throw_Exception_When_AcceptedBy_Is_Null()
        {
            // Arrange
            var invitation = UserInvitation.CreateDefaultInvitation(1, "test@example.com", new User { Id = 1, EmailAddress = "inviter@example.com" });

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => invitation.Accept(null));
        }

        [Fact]
        public void Accept_Should_Throw_Exception_When_Email_Does_Not_Match()
        {
            // Arrange
            var invitation = UserInvitation.CreateDefaultInvitation(1, "test@example.com", new User { Id = 1, EmailAddress = "inviter@example.com" });

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => invitation.Accept(new User { Id = 2, EmailAddress = "wrong@example.com" }));
        }

        [Fact]
        public void Accept_Should_Set_AcceptedDate_And_Status()
        {
            // Arrange
            var invitation = UserInvitation.CreateDefaultInvitation(1, "test@example.com", new User { Id = 1, EmailAddress = "inviter@example.com" });
            var user = new User { Id = 2, EmailAddress = "test@example.com" };

            // Act
            invitation.Accept(user);

            // Assert
            Assert.Equal(InvitationStatus.Accepted, invitation.Status);
            Assert.NotNull(invitation.AcceptedDate);
        }

        [Fact]
        public void Reactivate_Should_Throw_Exception_When_Invitation_Is_Null()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => UserInvitation.Reactivate(null, new User { Id = 1, EmailAddress = "reactivator@example.com" }));
        }

        [Fact]
        public void Reactivate_Should_Set_Status_And_ExpirationDate_And_Token()
        {
            // Arrange
            var invitation = UserInvitation.CreateDefaultInvitation(1, "test@example.com", new User { Id = 1, EmailAddress = "inviter@example.com" });
            var reactivatedBy = new User { Id = 2, EmailAddress = "reactivator@example.com" };

            // Act
            var reactivatedInvitation = UserInvitation.Reactivate(invitation, reactivatedBy);

            // Assert
            Assert.Equal(InvitationStatus.Pending, reactivatedInvitation.Status);
            Assert.True(reactivatedInvitation.ExpirationDate > Clock.Now);
            Assert.NotNull(reactivatedInvitation.Token);
        }
    }
}