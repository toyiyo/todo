using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Timing;
using toyiyo.todo.Authorization.Users;
using toyiyo.todo.Projects;

namespace toyiyo.todo.Jobs
{
    public class Job : FullAuditedEntity<Guid>, IMustHaveTenant
    {
        public enum Status {Open, InProgress, Done };
        public const int MaxTitleLength = 500; //todo: max length should be defined in the configuration
        //note: protected setter forces users to use "Set..." methods to set the value
        [Required]
        public Project Project { get; protected set; }
        [Required]
        [StringLength(MaxTitleLength)]
        public string Title { get; protected set; }
        public string Description {get; protected set;}
        public DateTime DueDate { get; protected set; }
        public User Owner { get; protected set; }
        public User Assignee { get; protected set; }
        //todo: reintroduce members many to many relationship later
        //public virtual ICollection<User> Members { get; protected set; }
        public Status JobStatus {get; protected set;}
        [Required]
        public virtual int TenantId { get; set; }

        /// <summary>
        /// We don't make constructor public and forcing to create events using <see cref="Create"/> method.
        /// But constructor can not be private since it's used by EntityFramework.
        /// Thats why we did it protected.
        /// </summary>
        protected Job()
        {

        }

        public static Job Create(Project project, string title, string description, User user, int tenantId)
        {
            if (title == null || user == null || tenantId <= 0 || project == null)
            {
                throw new ArgumentNullException(nameof(title) + " " + nameof(user) + " " + nameof(tenantId) + " " + nameof(project));
            }

            var job = new Job
            {
                Project = project,
                Title = title,
                Description = description,
                TenantId = tenantId,
                Owner = user,
                Assignee = user,
                //Members = new List<User>() {user},
                CreatorUserId = user.Id,
                LastModifierUserId = user.Id,
                CreationTime = Clock.Now,
                LastModificationTime = Clock.Now,
                JobStatus = Status.Open
            };

            return job;
        }

        public static Job SetTitle(Job job, string title, User user)
        {
            //validate parameters
            if (job == null || title == null || user == null)
            {
                throw new ArgumentNullException(nameof(job) + " " + nameof(title) + " " + nameof(user));
            }

            job.Title = title;
            job.LastModificationTime = Clock.Now;
            job.LastModifierUserId = user.Id;

            return job;
        }

        public static Job SetDescription(Job job, string description, User user)
        {
            //validate parameters
            if (job == null || description == null || user == null)
            {
                throw new ArgumentNullException(nameof(job) + " " + nameof(description) + " " + nameof(user));
            }

            job.Description = description;
            job.LastModificationTime = Clock.Now;
            job.LastModifierUserId = user.Id;

            return job;
        }

        public static Job SetStatus(Job job, Status status, User user)
        {
            //validate parameters
            if (job == null || user == null)
            {
                throw new ArgumentNullException(nameof(job) + " " + nameof(user));
            }

            job.JobStatus = status;
            job.LastModificationTime = Clock.Now;
            job.LastModifierUserId = user.Id;

            return job;
        }

        public static Job SetDueDate(Job job, DateTime dueDate, User user)
        {
            //validate parameters
            if (job == null || dueDate == null || user == null)
            {
                throw new ArgumentNullException(nameof(job) + " " + nameof(dueDate) + " " + nameof(user));
            }

            if (dueDate < DateTime.Now)
            {
                throw new ArgumentOutOfRangeException(nameof(dueDate));
            }

            job.DueDate = dueDate;
            job.LastModificationTime = Clock.Now;
            job.LastModifierUserId = user.Id;

            return job;
        }
    }
}