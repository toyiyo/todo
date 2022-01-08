using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Timing;
using toyiyo.todo.Authorization.Users;

namespace toyiyo.todo.Projects
{
    public class Project : FullAuditedEntity<Guid>, IMustHaveTenant
    {
        public const int MaxTitleLength = 500; //todo: max length should be defined in the configuration
        [Required]
        [StringLength(MaxTitleLength)]
        public string Title { get; set; }
        [Required]
        public virtual int TenantId { get; set; }

        /// <summary>
        /// We don't make constructor public and forcing to create events using <see cref="Create"/> method.
        /// But constructor can not be private since it's used by EntityFramework.
        /// Thats why we did it protected.
        /// </summary>
        protected Project()
        {

        }

        public static Project Create(string title, User user, int tenantId)
        {
            var @project = new Project
            {
                Id = Guid.NewGuid(),
                Title = title,
                TenantId = tenantId,
                CreatorUserId = user.Id,
                LastModifierUserId = user.Id,
                CreationTime = Clock.Now,
                LastModificationTime = Clock.Now
            };

            return @project;
        }
    }
}