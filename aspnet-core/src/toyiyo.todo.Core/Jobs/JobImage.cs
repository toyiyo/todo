using System;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using toyiyo.todo.Jobs;
using Abp.Timing;
using toyiyo.todo.Authorization.Users;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

namespace toyiyo.todo.Jobs
{
    [Index(nameof(ContentHash), IsUnique = true)]
    public class JobImage : FullAuditedEntity<Guid>, IMustHaveTenant
    {
        [Required]
        public int TenantId { get; set; }
        [Required]
        public Guid JobId { get; protected set; }
        [Required]
        [StringLength(255)]
        public string ContentType { get; protected set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; protected set; }
        [Required]
        public byte[] ImageData { get; protected set; }

        [Required]
        [StringLength(64)]
        public string ContentHash { get; protected set; }

        public static JobImage Create(Job job, string ContentType, string FileName, byte[] imageData, int tenantId, User user)
        {
            if (job == null) { throw new ArgumentNullException(nameof(job)); }
            if (imageData == null || imageData.Length == 0) { throw new ArgumentNullException(nameof(imageData)); }
            if (user == null) { throw new ArgumentNullException(nameof(user)); }
            if (job.TenantId != tenantId) { throw new ArgumentOutOfRangeException(nameof(tenantId), "tenant mismatch"); }

            var image = new JobImage
            {
                JobId = job.Id,
                ContentType = ContentType,
                FileName = FileName,
                TenantId = tenantId,
                CreatorUserId = user.Id,
                CreationTime = Clock.Now,
                LastModifierUserId = user.Id,
                LastModificationTime = Clock.Now,
                ContentHash = ComputeHash(imageData)
            };

            SetImageData(image, imageData, user);
            return image;
        }

        public static JobImage SetImageData(JobImage image, byte[] imageData, User user)
        {
            if (image == null || imageData == null || imageData.Length == 0)
            {
                throw new ArgumentNullException(nameof(image));
            }
            if (user == null) { throw new ArgumentNullException(nameof(user)); }
            image.ImageData = imageData;
            SetLastModified(image, user);
            return image;
        }


        public static JobImage Delete(JobImage image, User user)
        {
            if (image == null || user == null)
            {
                throw new ArgumentNullException();
            }
            image.IsDeleted = true;
            image.DeletionTime = Clock.Now;
            image.DeleterUserId = user.Id;
            return image;
        }

        private static void SetLastModified(JobImage image, User user)
        {
            image.LastModificationTime = Clock.Now;
            image.LastModifierUserId = user.Id;
        }

        public static string ComputeHash(byte[] imageData)
        {
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(imageData);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}