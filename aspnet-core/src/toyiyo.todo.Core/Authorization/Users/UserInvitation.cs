using System;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Timing;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace toyiyo.todo.Authorization.Users
{
    [Index(nameof(Email))]
    [Index(nameof(Token))]
    [Index(nameof(IsActive))]
    public class UserInvitation : FullAuditedEntity<Guid>, IMustHaveTenant
    {
        private const int TOKEN_LENGTH = 64; // 512 bits
        private const int DEFAULT_EXPIRATION_DAYS = 7;
        private const int MAX_EMAIL_LENGTH = 256;
        public virtual int TenantId { get; set; }
        [Required]
        [EmailAddress]
        [MaxLength(MAX_EMAIL_LENGTH)]
        public string Email { get; protected set; }
        // Keep as ID to avoid navigation property complexity
        [Required]
        public long InvitedByUserId { get; protected set; }

        // Add navigation property but make it virtual for lazy loading
        public virtual User InvitedBy { get; protected set; }
        [Required]
        public DateTime ExpirationDate { get; protected set; }
        public DateTime? AcceptedDate { get; protected set; }
        [Required]
        public string Token { get; protected set; }
        [Required]
        public bool IsActive { get; protected set; }
        [Required]
        public InvitationStatus Status {get; protected set;}

        /// <summary>
        /// We don't make constructor public and forcing to create events using <see cref="Create"/> method.
        /// But constructor can not be private since it's used by EntityFramework.
        /// Thats why we did it protected.
        /// </summary>
        protected UserInvitation() { }

        private static UserInvitation Create(int tenantId, string email, long invitedByUserId, DateTime expirationDate, string token, User user, InvitationStatus status = InvitationStatus.Pending)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentNullException(nameof(email));
            if (!IsValidEmail(email)){
                throw new ArgumentException("Email is not valid", nameof(email));
            }
            if (invitedByUserId <= 0)
                throw new ArgumentNullException(nameof(invitedByUserId));
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (expirationDate <= Clock.Now)
                throw new ArgumentException("Expiration date must be in the future", nameof(expirationDate));

            var userInvitation = new UserInvitation
            {
                TenantId = tenantId,
                Email = email.Trim().ToLowerInvariant(),
                InvitedByUserId = invitedByUserId,
                ExpirationDate = expirationDate,
                Token = token,
                IsActive = true,
                CreatorUserId = user.Id,
                CreationTime = Clock.Now
            };

            SetLastModified(userInvitation, user);
            return userInvitation;
        }
        public bool IsExpired() => ExpirationDate < Clock.Now;
        public bool IsValid() => IsActive && !IsExpired() && Status == InvitationStatus.Pending;
        public bool ValidateToken(string token) => Token.Equals(token, StringComparison.Ordinal) && IsValid();
        public static DateTime GetDefaultExpirationDate() => Clock.Now.AddDays(DEFAULT_EXPIRATION_DAYS);

        public static string GenerateToken()
        {
            var bytes = new byte[TOKEN_LENGTH];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
        }

        private static bool IsValidEmail(string email) {
            try {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            } catch {
                return false;
            }
        }

        private static void SetLastModified(UserInvitation userInvitation, User user)
        {
            userInvitation.LastModificationTime = Clock.Now;
            userInvitation.LastModifierUserId = user.Id;
        }
        public static UserInvitation CreateDefaultInvitation(int tenantId, string email, User invitedBy)
        {
            return Create(
                tenantId,
                email,
                invitedBy.Id,
                GetDefaultExpirationDate(),
                GenerateToken(),
                invitedBy
            );
        }

        public void Accept(User acceptedBy)
        {
        if (acceptedBy == null)
            throw new ArgumentNullException(nameof(acceptedBy));
            
        if (!IsValid())
            throw new InvalidOperationException("Invitation is not valid");
            
        if (!acceptedBy.EmailAddress.Equals(Email, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Email address does not match invitation");

            AcceptedDate = Clock.Now;
            IsActive = false;
            Status = InvitationStatus.Accepted;
            SetLastModified(this, acceptedBy);
        }

    }
    public enum InvitationStatus
    {
        Pending = 0,
        Accepted = 1,
        Expired = 2,
        Cancelled = 3
    }
}

