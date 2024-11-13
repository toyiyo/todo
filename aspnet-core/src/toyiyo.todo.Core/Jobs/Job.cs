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
    [Index(nameof(JobStatus), nameof(Level))]
    public class Job : FullAuditedEntity<Guid>, IMustHaveTenant
    {
        //doing this as an enum means any new status will require a code change, consider using a lookup table in the future
        public enum Status { Open, InProgress, Done };
        public enum JobLevel { Task, SubTask, Epic, Bug };
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
        public JobLevel Level { get; protected set; }
        public Guid ParentId { get; protected set; }
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

        /// <summary>
        /// Creates a new job with the specified parameters.
        /// </summary>
        /// <param name="project">The project the job belongs to.</param>
        /// <param name="title">The title of the job.</param>
        /// <param name="description">The description of the job.</param>
        /// <param name="user">The user who created the job.</param>
        /// <param name="tenantId">The ID of the tenant the job belongs to.</param>
        /// <param name="dueDate">The due date of the job (optional).</param>
        /// <param name="parentId">The ID of the parent job (optional).</param>
        /// <param name="jobLevel">The level of the job (optional). Defaults to task.  Options are Task, SubTask, Epic</param>
        /// <returns>The newly created job.</returns>
        public static Job Create(Project project, string title, string description, User user, int tenantId, DateTime dueDate = default, Guid parentId = default, JobLevel jobLevel = 0)
        {
            if (title == null) { throw new ArgumentNullException(nameof(title)); }
            if (user == null) { throw new ArgumentNullException(nameof(user)); }
            if (tenantId <= 0) { throw new ArgumentNullException(nameof(tenantId)); }
            if (project == null) { throw new ArgumentNullException(nameof(project)); }
            if ((dueDate != default) && (dueDate.Kind != DateTimeKind.Utc ? dueDate.ToUniversalTime().Date : dueDate.Date) < DateTime.UtcNow.Date) {throw new ArgumentOutOfRangeException(nameof(dueDate), "due date must be in the future"); }
            if (jobLevel == JobLevel.Epic && parentId != default) { throw new ArgumentOutOfRangeException(nameof(parentId), "epics cannot have parents"); }

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
                ParentId = parentId,
                OrderByDate = Clock.Now,
                Level = jobLevel
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
            if (job == null) { throw new ArgumentNullException(nameof(job)); }
            if (user == null) { throw new ArgumentNullException(nameof(user)); }

            job.JobStatus = status;
            SetLastModified(job, user);

            return job;
        }

        public static Job SetDueDate(Job job, DateTime dueDate, User user)
        {
            //due dates can be set to null, but if they are set, they must be in the future
            if (job == null) { throw new ArgumentNullException(nameof(job)); }
            if (user == null) { throw new ArgumentNullException(nameof(user)); }
            if ((dueDate != default) && (dueDate < Clock.Now.Date)) { throw new ArgumentOutOfRangeException(nameof(dueDate), "due date must be in the future"); }

            job.DueDate = dueDate;
            SetLastModified(job, user);

            return job;
        }

        public static Job SetParent(Job job, Job parentJob, User user)
        {
            Guid parentId = parentJob == null ? default : parentJob.Id;
            if (job == null) { throw new ArgumentNullException(nameof(job)); }
            if (user == null) { throw new ArgumentNullException(nameof(user)); }
            if (parentJob != null && job.Project.Id != parentJob.Project.Id) { throw new ArgumentOutOfRangeException(nameof(parentJob), "parent job must be in the same project"); }
            if (job.Level == JobLevel.Epic && parentId != default) { throw new ArgumentOutOfRangeException(nameof(parentId), "epics cannot have parents"); }
            if (job.ParentId == parentId) { return job; }
            job.ParentId = parentId;
            SetLastModified(job, user);

            return job;
        }

        public static Job SetLevel(Job job, JobLevel level, User user)
        {
            if (job == null) { throw new ArgumentNullException(nameof(job)); }
            if (user == null) { throw new ArgumentNullException(nameof(user)); }
            if (level == JobLevel.Epic && job.ParentId != default) 
            { 
                throw new ArgumentOutOfRangeException(nameof(level), "epics cannot have parents"); 
            }
            job.Level = level;
            SetLastModified(job, user);
            return job;
        }
    }
}