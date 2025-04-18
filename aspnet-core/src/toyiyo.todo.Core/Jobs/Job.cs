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
using System.Collections.Generic;

namespace toyiyo.todo.Jobs
{
    [Index(nameof(JobStatus), nameof(Level), nameof(TenantId))]
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
        public Status JobStatus { get; protected set; }
        public JobLevel Level { get; protected set; }
        public Guid ParentId { get; protected set; }
        [Required]
        public virtual int TenantId { get; set; }
        //our default ordering is by date created, give we don't have all the values in the DB, we are returning a default value in code
        public DateTime OrderByDate { get { return (_orderByDate == DateTime.MinValue) ? CreationTime : _orderByDate; } protected set { _orderByDate = value; } }

        // Start date for roadmap visualization
        public DateTime? StartDate { get; protected set; }
    
        // Dependencies collection
        public virtual ICollection<Job> Dependencies { get; protected set; }

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
        /// <param name="startDate">The start date of the job (optional).</param>
        /// <returns>The newly created job.</returns>
        public static Job Create(Project project, string title, string description, User user, int tenantId, DateTime dueDate = default, Guid parentId = default, JobLevel jobLevel = 0, DateTime? startDate = null)
        {
            if (user == null) { throw new ArgumentNullException(nameof(user)); }
            if (tenantId <= 0) { throw new ArgumentNullException(nameof(tenantId)); }
            if (project == null) { throw new ArgumentNullException(nameof(project)); }

            var job = new Job
            {
                Project = project,
                TenantId = tenantId,
                Owner = user,
                CreatorUserId = user.Id,
                LastModifierUserId = user.Id,
                CreationTime = Clock.Now,
                LastModificationTime = Clock.Now,
                JobStatus = Status.Open,
                OrderByDate = Clock.Now
            };

            // Use existing setter methods for validation
            SetTitle(job, title, user);
            SetDescription(job, description, user);
            SetDueDate(job, dueDate, user);
            SetStartDate(job, startDate, user);
            SetLevel(job, jobLevel, user);
            
            // Set parent last since it depends on the level being set
            if (parentId != default)
            {
                job.ParentId = parentId; // Direct assignment since we don't have the parent Job object here
            }

            return job;
        }

        public static Job SetTitle(Job job, string title, User user)
        {
            //validate parameters
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }
            if (title == null)
            {
                throw new ArgumentNullException(nameof(title));
            }
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
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
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
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
            if (!Enum.IsDefined(typeof(JobLevel), level))
            {
                throw new ArgumentException("Invalid job level", nameof(level));
            }
            job.Level = level;
            SetLastModified(job, user);
            return job;
        }

        public static Job SetAssignee(Job job, User assignee, User user)
        {
            if (job == null) { throw new ArgumentNullException(nameof(job)); }
            if (user == null) { throw new ArgumentNullException(nameof(user)); }
            if (assignee != null && job.Project.TenantId != assignee.TenantId) 
            { 
                throw new ArgumentOutOfRangeException(nameof(assignee), "assignee must be in the same tenant"); 
            }
            if (assignee != null && !assignee.IsActive) {throw new ArgumentOutOfRangeException(nameof(assignee), "assignee must be active");}
            if (job.JobStatus == Status.Done) { throw new ArgumentOutOfRangeException("Cannot assign a job that is done", nameof(job.JobStatus)); }
            job.Assignee = assignee; //allowing null assignee
            SetLastModified(job, user);
            return job;
        }

        public static Job SetStartDate(Job job, DateTime? startDate, User user)
        {
            if (job == null) { throw new ArgumentNullException(nameof(job)); }
            if (user == null) { throw new ArgumentNullException(nameof(user)); }
            if (startDate.HasValue && startDate.Value > job.DueDate) 
            { 
                throw new ArgumentException("Start date must be before due date"); 
            }

            job.StartDate = startDate;
            SetLastModified(job, user);
            return job;
        }

        public static Job AddDependency(Job job, Job dependency, User user)
        {
            if (job == null) { throw new ArgumentNullException(nameof(job)); }
            if (dependency == null) { throw new ArgumentNullException(nameof(dependency)); }
            if (user == null) { throw new ArgumentNullException(nameof(user)); }
            if (job.TenantId != dependency.TenantId) 
            { 
                throw new ArgumentException("Dependencies must be in the same tenant"); 
            }

            if (job.Dependencies == null)
            {
                job.Dependencies = new List<Job>();
            }

            if (HasCircularDependency(job, dependency))
            {
                throw new InvalidOperationException("Circular dependency detected");
            }

            if (!job.Dependencies.Contains(dependency))
            {
                job.Dependencies.Add(dependency);
                SetLastModified(job, user);
            }

            return job;
        }

        private static bool HasCircularDependency(Job job, Job dependency)
        {
            if (job == null || dependency == null) return false;
            if (job.Dependencies == null) return false;

            var visited = new HashSet<Job>();
            var stack = new Stack<Job>();
            stack.Push(job);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (current.Dependencies == null) continue;

                foreach (var dep in current.Dependencies)
                {
                    if (dep == dependency) return true;
                    if (visited.Add(dep))
                    {
                        stack.Push(dep);
                    }
                }
            }

            return false;
        }

        public static Job RemoveDependency(Job job, Job dependency, User user)
        {
            if (job == null) { throw new ArgumentNullException(nameof(job)); }
            if (dependency == null) { throw new ArgumentNullException(nameof(dependency)); }
            if (user == null) { throw new ArgumentNullException(nameof(user)); }

            if (job.Dependencies != null && job.Dependencies.Contains(dependency))
            {
                job.Dependencies.Remove(dependency);
                SetLastModified(job, user);
            }

            return job;
        }

        public static Job SetDates(Job job, DateTime startDate, DateTime dueDate, User user)
        {
            if (job == null) { throw new ArgumentNullException(nameof(job)); }
            if (user == null) { throw new ArgumentNullException(nameof(user)); }
            if (startDate > dueDate) { throw new ArgumentException("Start date must be before due date"); }

            job.StartDate = startDate;
            job.DueDate = dueDate;
            SetLastModified(job, user);
            return job;
        }
    }
}