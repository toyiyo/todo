using System;
using Abp;
using Abp.Dependency;
using Abp.UI;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Timing;
using Microsoft.EntityFrameworkCore;
using toyiyo.todo.Authorization.Users;
using toyiyo.todo.Projects;

namespace toyiyo.todo.Jobs
{
    [Index(nameof(JobStatus))]
    public class Job : FullAuditedEntity<Guid>, IMustHaveTenant
    {
        public enum Status { Open, InProgress, Done };
        private DateTime _orderByDate;
        public const int MaxTitleLength = 500; //todo: max length should be defined in the configuration
        public const int MaxDescriptionLength = 2000000; // 2MB limit | 307692 - 400000 words | 1230.8 - 1600.0 pages

        //note: protected setter forces users to use "Set..." methods to set the value
        [Required]
        public Project Project { get; protected set; }
        [Required]
        [StringLength(MaxTitleLength)]
        public string Title { get; protected set; }
        [StringLength(MaxDescriptionLength)]
        public string Description { get; protected set; }
        public DateTime DueDate { get; protected set; }
        public User Owner { get; protected set; }
        public User Assignee { get; protected set; }
        //todo: reintroduce members many to many relationship later
        //public virtual ICollection<User> Members { get; protected set; }
        public Status JobStatus { get; protected set; }
        [Required]
        public virtual int TenantId { get; set; }
        //our default ordering is by date created, give we don't have all the values in the DB, we are returning a default value in code
        public DateTime OrderByDate { get { return (_orderByDate == DateTime.MinValue) ? CreationTime : _orderByDate; } protected set { _orderByDate = value; } }

        /// <summary>
        /// We don't make constructor public and forcing to create events using <see cref="Create"/> method.
        /// But constructor can not be private since it's used by EntityFramework.
        /// Thats why we did it protected.
        /// </summary>
        protected Job()
        {

        }

        public static Job Create(Project project, string title, string description, User user, int tenantId, DateTime dueDate = default)
        {
            if (title == null) { throw new ArgumentNullException(nameof(title)); }
            if (user == null) { throw new ArgumentNullException(nameof(user)); }
            if (tenantId <= 0) { throw new ArgumentNullException(nameof(tenantId)); }
            if (project == null) { throw new ArgumentNullException(nameof(project)); }
            if (dueDate < Clock.Now.Date) { throw new ArgumentOutOfRangeException(nameof(dueDate), "due date must be in the future"); }

            var job = new Job
            {
                Project = project,
                Title = title,
                Description = description,
                TenantId = tenantId,
                Owner = user,
                Assignee = user,
                CreatorUserId = user.Id,
                LastModifierUserId = user.Id,
                CreationTime = Clock.Now,
                LastModificationTime = Clock.Now,
                JobStatus = Status.Open,
                OrderByDate = Clock.Now
            };
            if (dueDate != default) { job.DueDate = dueDate; }

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
            SetLastModified(job, user);

            return job;
        }

        public static Job SetOrderByDate(Job job, User user, DateTime orderByDate)
        {
            if (job == null) { throw new ArgumentNullException(nameof(job)); }
            if (user == null) { throw new ArgumentNullException(nameof(user)); }
            if (orderByDate == default) { throw new ArgumentNullException(nameof(orderByDate)); }

            job.OrderByDate = orderByDate;
            SetLastModified(job, user);
            return job;
        }

        public static Job SetDescription(Job job, string description, User user)
        {
            //validate parameters
            if (job == null || user == null)
            {
                throw new ArgumentNullException(nameof(job) + " " + nameof(user));
            }

            job.Description = description;
            SetLastModified(job, user);

            return job;
        }

        public static Job Delete(Job job, User user)
        {
            //at present, anyone with account access can delete, consider limiting delete operations to owners or assignees to the job.
            if (job == null) { throw new ArgumentNullException(nameof(job)); }
            if (user == null) { throw new ArgumentNullException(nameof(user)); }

            SetLastModified(job, user);
            job.IsDeleted = true;
            job.DeletionTime = Clock.Now;
            job.DeleterUserId = user.Id;

            return job;
        }

        private static void SetLastModified(Job job, User user)
        {
            job.LastModificationTime = Clock.Now;
            job.LastModifierUserId = user.Id;
        }

        public static Job SetStatus(Job job, Status status, User user)
        {
            //validate parameters
            if (job == null || user == null)
            {
                throw new ArgumentNullException(nameof(job) + " " + nameof(user));
            }

            job.JobStatus = status;
            SetLastModified(job, user);

            return job;
        }

        public static Job SetDueDate(Job job, DateTime dueDate, User user)
        {
            //validate parameters
            if (job == null || dueDate == DateTime.MinValue || user == null)
            {
                throw new ArgumentNullException(nameof(job) + " " + nameof(dueDate) + " " + nameof(user));
            }

            if (dueDate < DateTime.Now)
            {
                throw new ArgumentOutOfRangeException(nameof(dueDate));
            }

            job.DueDate = dueDate;
            SetLastModified(job, user);

            return job;
        }


    }
}