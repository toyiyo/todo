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
        //note: protected setter forces users to use "Set..." methods to set the value
        [Required]
        [StringLength(MaxTitleLength)]
        public string Title { get; protected set; }
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
            //validate parameters
            if (title == null || user == null || tenantId <= 0)
            {
                throw new ArgumentNullException("title, user and tenantId are required");
            }

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

        public static Project SetTitle(Project project, string title, User user)
        {
            //validate parameters
            if (project == null || title == null || user == null)
            {
                throw new ArgumentNullException("project, title and user are required");
            }

            project.Title = title;
            project.LastModificationTime = Clock.Now;
            project.LastModifierUserId = user.Id;

            return project;
        }
    }
}