# .github/copilot-instructions.yml

# Repository Description

Toyiyo Todo Application is a modern project management tool built with ASP.NET Core and ASP.NET Boilerplate framework. Following Domain-Driven Design (DDD) principles, it implements a complete n-layer architecture with clear separation of concerns.

# Architecture Overview

## Layer Organization

1. Domain Layer (Core)
   - Entities and value objects
   - Domain services
   - Domain interfaces
   - Business rules and validations
   - Domain events

2. Application Layer
   - Application services
   - DTOs and mapping
   - Use case orchestration
   - Transaction handling

3. Infrastructure Layer
   - Repository implementations
   - Database context
   - External integrations
   - Cross-cutting concerns

4. Presentation Layer
   - MVC Controllers
   - Views and ViewModels
   - JavaScript/frontend
   - UI Components

## Layer Dependencies
```
Presentation Layer
       ↓
Application Layer
       ↓
Domain Layer
       ↑
Infrastructure Layer
```

## Complete Example: Forecasting Feature

### 1. Domain Layer

#### Value Objects
```csharp
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
```

#### Domain Manager Interface
```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using toyiyo.todo.Authorization.Users;

namespace toyiyo.todo.Jobs
{
    public interface IJobManager
    {
        Task<Job> Create(Job inputJob);
        Task<Job> Get(Guid id);
        Task<List<Job>> GetAll(GetAllJobsInput input);

        Task<int> GetAllCount(GetAllJobsInput input);
        Task<Job> Update(Job inputJob);
        Task Delete(Guid id, User user);
        Task<Job> SetOrderByDate(Guid id, User user, DateTime orderByDate);
    }
}
```

#### Domain Manager Implementation
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using toyiyo.todo.Authorization.Users;

namespace toyiyo.todo.Jobs
{
    public class JobManager : DomainService, IJobManager
    {
        public readonly IRepository<Job, Guid> _jobRepository;
        public JobManager(IRepository<Job, Guid> jobRepository)
        {
            _jobRepository = jobRepository;
        }

        public async Task<Job> Get(Guid id)
        {
            return await _jobRepository.Query(q => q.Where(p => p.Id == id)
                        .Include(p => p.Project)
                        .Include(p => p.Assignee)
                        .Include(p => p.Owner)
                        .FirstOrDefaultAsync());

        }
        //GetAll() repository method requires a unit of work to be open. see https://aspnetboilerplate.com/Pages/Documents/Unit-Of-Work#irepository-getall-method
        [UnitOfWork]
        public async Task<List<Job>> GetAll(GetAllJobsInput input)
        {
            return await GetAllJobsQueryable(input)
            .Include(p => p.Project)
            .Include(p => p.Assignee)
            .Include(p => p.Owner)
            .OrderBy<Job>(input?.Sorting ?? "OrderByDate DESC")
            .Skip(input?.SkipCount ?? 0)
            .Take(input?.MaxResultCount ?? int.MaxValue)
            .ToListAsync();
        }

        [UnitOfWork]
        public async Task<int> GetAllCount(GetAllJobsInput input)
        {
            return await GetAllJobsQueryable(input).CountAsync();
        }

        private IQueryable<Job> GetAllJobsQueryable(GetAllJobsInput input)
        {
            //repository methods already filter by tenant, we can check other attributes by adding "or" "||" to the whereif clause
            var query = _jobRepository.GetAll()
                .WhereIf(!input.ProjectId.Equals(Guid.Empty), x => x.Project.Id == input.ProjectId)
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), p => p.Title.ToUpper().Contains(input.Keyword.ToUpper()) || p.Description.ToUpper().Contains(input.Keyword.ToUpper()))
                .WhereIf(input.JobStatus != null, p => p.JobStatus == input.JobStatus)
                .WhereIf(!input.ParentJobId.Equals(Guid.Empty), p => p.ParentId == input.ParentJobId)
                .WhereIf(input.Levels != null && input.Levels.Any(), p => input.Levels.Contains(p.Level));

            return query;
        }

        public async Task<Job> Create(Job inputJob)
        {
            var job = await _jobRepository.InsertAsync(inputJob);
            return job;
        }

        [UnitOfWork]
        public async Task<Job> Update(Job inputJob)
        {
            var job = await _jobRepository.UpdateAsync(inputJob);
            return job;
        }

        public async Task Delete(Guid id, User user)
        {
            var job = Job.Delete(await this.Get(id), user);
            await _jobRepository.DeleteAsync(job);
        }

        public async Task<Job> SetOrderByDate(Guid id, User user, DateTime orderByDate)
        {
            //the manager is ust a pass through to instantiate the correct class to handle the logic
            //For our domain, the Job class contains the domain logic
            var job = Job.SetOrderByDate(await Get(id), user, orderByDate);
            //Our repository class already filters by tenant, so when getting the job by id, if the current user doesn't have access, the job will be empty and no update will happen
            return await _jobRepository.UpdateAsync(job);
        }
    }
}
```

### 2. Application Layer

#### DTOs
```csharp
using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;

namespace toyiyo.todo.Jobs
{
    [AutoMap(typeof(JobImage))]
    public class JobImageDto : EntityDto<Guid>
    {
        public Guid JobId { get; set; }
        public string ContentType { get; set; }
        public string FileName { get; set; }
        public string imageUrl { get; set; }
    }
}
```

#### Application Service
```csharp
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using toyiyo.todo.Authorization;
using toyiyo.todo.Authorization.Users;
using toyiyo.todo.Jobs.Dto;
using toyiyo.todo.Projects;
using static toyiyo.todo.Jobs.Job;

namespace toyiyo.todo.Jobs
{
    [AbpAuthorize(PermissionNames.Pages_Jobs)]
    public class JobAppService : todoAppServiceBase, IJobAppService
    {
        private readonly IJobManager _jobManager;
        private readonly IProjectManager _projectManager;
        private readonly UserManager _userManager;
        private readonly IMarkdownImageExtractor _imageExtractor;
        private readonly IJobImageManager _jobImageManager;
        private readonly IJobImageAppService _jobImageAppService;

        public JobAppService(IJobManager jobManager, IProjectManager projectManager, UserManager userManager, IMarkdownImageExtractor imageExtractor, IJobImageManager jobImageManager, IJobImageAppService jobImageAppService)
        {
            _jobManager = jobManager;
            _projectManager = projectManager;
            _userManager = userManager;
            _imageExtractor = imageExtractor;
            _jobImageManager = jobImageManager;
            _jobImageAppService = jobImageAppService;
        }
        /// <summary> Creates a new Job. </summary>
        public async Task<JobDto> Create(JobCreateInputDto input)
        {
            var tenant = await GetCurrentTenantAsync();
            var project = await _projectManager.Get(input.ProjectId);
            var job = Job.Create(project, input.Title, input.Description, await GetCurrentUserAsync(), tenant.Id, input.DueDate ?? default, input.ParentId ?? default, input.Level, input.StartDate);
            await _jobManager.Create(job);
            return ObjectMapper.Map<JobDto>(job);
        }
        /// <summary> Gets a Job by Id. </summary>
        public async Task<JobDto> Get(Guid id)
        {
            var job = await _jobManager.Get(id);
            return ObjectMapper.Map<JobDto>(job);
        }
        /// <summary> Gets all Jobs. Keyword filters by Title</summary>
        public async Task<PagedResultDto<JobDto>> GetAll(GetAllJobsInput input)
        {
            var jobs = await _jobManager.GetAll(input);
            var jobsTotalCount = await _jobManager.GetAllCount(input);
            return new PagedResultDto<JobDto>(jobsTotalCount, ObjectMapper.Map<List<JobDto>>(jobs));
        }

        public async Task<JobStatsDto> GetJobStats()
        {
            //getting stats for the account, for the future, we should allow filtering by project.
            //we'll manually remove subtasks from the stats count for now.  
            //todo, once we have a job type defined, we can filter by job type
            var getAllJobsInput = new GetAllJobsInput() { MaxResultCount = int.MaxValue, Levels = new List<JobLevel> { JobLevel.Task, JobLevel.Bug }.ToArray() };
            var jobs = await GetAll(getAllJobsInput);
            return new JobStatsDto
            {
                TotalJobs = jobs.Items.Count(),
                TotalOpenJobs = jobs.Items.Count(x => x.JobStatus == Job.Status.Open),
                TotalInProgressJobs = jobs.Items.Count(x => x.JobStatus == Job.Status.InProgress),
                TotalCompletedJobs = jobs.Items.Count(x => x.JobStatus == Job.Status.Done),
                TotalCompletedJobsPerMonth = //return job items grouped by date and count
                jobs.Items
                    .Where(x => x.JobStatus == Job.Status.Done)
                    .GroupBy(x => new { x.LastModificationTime.Value.Year, x.LastModificationTime.Value.Month })
                    .Select(x => new JobStatsPerMonthDto
                    {
                        Year = x.Key.Year,
                        Month = DateTimeFormatInfo.CurrentInfo.MonthNames[x.Key.Month - 1],
                        Count = x.Count()
                    }).ToList()
            };
        }

        public async Task<JobDto> SetDescription(JobSetDescriptionInputDto input)
        {
            var currentUser = await GetCurrentUserAsync();
            var job = await _jobManager.Get(input.Id);

            // Extract and save images
            string imageCleanedDescription = await ExtractAndReplaceImagesInDescription(input.Description, job);
            job = Job.SetDescription(job, imageCleanedDescription, currentUser);
            await _jobManager.Update(job);

            return ObjectMapper.Map<JobDto>(job);
        }

        private async Task<string> ExtractAndReplaceImagesInDescription(string description, Job job)
        {
            var images = _imageExtractor.ExtractImages(description);
            var imageIdMap = new Dictionary<string, string>();

            foreach (var img in images)
            {
                var imageData = Convert.FromBase64String(img.Base64Data);

                var jobImageCreateInputDto = new JobImageCreateInputDto
                {
                    JobId = job.Id,
                    ContentType = img.ContentType,
                    FileName = img.FileName,
                    ImageData = new FormFile(
                        new MemoryStream(imageData),
                        0,
                        imageData.Length,
                        img.FileName,
                        img.FileName
                    )
                };

                var savedImage = await _jobImageAppService.Create(jobImageCreateInputDto);
                imageIdMap.TryAdd(img.Base64Data, savedImage.imageUrl);

            }

            return _imageExtractor.ReplaceBase64ImagesWithUrls(description, imageIdMap);
        }

        public async Task<JobDto> SetJobStatus(JobSetStatusInputDto jobSetStatusInputDto)
        {
            var job = Job.SetStatus(await _jobManager.Get(jobSetStatusInputDto.Id), jobSetStatusInputDto.JobStatus, await GetCurrentUserAsync());
            await _jobManager.Update(job);
            return ObjectMapper.Map<JobDto>(job);
        }

        /// <summary> Sets the job's title </summary>
        public async Task<JobDto> SetTitle(JobSetTitleInputDto input)
        {
            var job = Job.SetTitle(await _jobManager.Get(input.Id), input.Title, await GetCurrentUserAsync());
            await _jobManager.Update(job);
            return ObjectMapper.Map<JobDto>(job);
        }

        public async Task<JobDto> SetDueDate(JobSetDueDateInputDto jobSetDueDateInputDto)
        {
            var job = Job.SetDueDate(await _jobManager.Get(jobSetDueDateInputDto.Id), jobSetDueDateInputDto.DueDate ?? default, await GetCurrentUserAsync());
            await _jobManager.Update(job);
            return ObjectMapper.Map<JobDto>(job);
        }

        //set the parentId of a job
        public async Task<JobDto> SetParent(JobSetParentInputDto jobSetParentInputDto)
        {
            var job = await _jobManager.Get(jobSetParentInputDto.Id);
            var parentJob = jobSetParentInputDto.ParentId == null || jobSetParentInputDto.ParentId == Guid.Empty ? null : await _jobManager.Get(jobSetParentInputDto.ParentId.Value);
            var user = await GetCurrentUserAsync();

            job = Job.SetParent(job, parentJob, user);
            await _jobManager.Update(job);

            return ObjectMapper.Map<JobDto>(job);
        }

        /// <summary>
        /// sets the order by date to manually sort
        /// </summary>
        /// <param name="jobPatchOrderByDateInputDto"></param>
        /// <returns>ob with updated order by date</returns>
        public async Task<ActionResult<JobDto>> PatchOrderByDate(JobPatchOrderByDateInputDto jobPatchOrderByDateInputDto)
        {
            //see more best practices in https://learn.microsoft.com/en-us/azure/architecture/best-practices/api-implementation
            try
            {
                var job = await _jobManager.SetOrderByDate(jobPatchOrderByDateInputDto.Id, await GetCurrentUserAsync(), jobPatchOrderByDateInputDto.OrderByDate);
                await _jobManager.Update(job);
                return ObjectMapper.Map<JobDto>(job);
            }
            catch (ArgumentNullException) { return new NotFoundResult(); }
            catch (Abp.Domain.Entities.EntityNotFoundException) { return new NotFoundResult(); }
        }
        /// <inheritdoc/>
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                //call the domain object's delete method so that our domain logic is checked, the get method gets the domain filtering already, so we don't delete jobs for tenants the user doesn't have access to
                //the domain object will perform validations and throw exceptions
                await _jobManager.Delete(id, await GetCurrentUserAsync());
                //https://learning.oreilly.com/library/view/rest-in-practice/9781449383312/ch04.html#delete_request_and_responses
                return new NoContentResult();
            }
            catch (System.ArgumentNullException) { return new NotFoundResult(); }
            catch (Abp.Domain.Entities.EntityNotFoundException) { return new NotFoundResult(); }
        }

        public async Task<JobDto> SetLevel(JobSetLevelInputDto input)
        {
            try
            {
                var job = Job.SetLevel(await _jobManager.Get(input.Id), input.Level, await GetCurrentUserAsync());
                await _jobManager.Update(job);
                return ObjectMapper.Map<JobDto>(job);
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(L("JobUpdateFailed"), ex.Message);
            }
        }

        public async Task<JobDto> SetAssignee(JobSetAssigneeInputDto input)
        {
            try
            {
                var job = Job.SetAssignee(await _jobManager.Get(input.Id), input.AssigneeId == null ? null : await _userManager.GetUserByIdAsync(input.AssigneeId.Value), await GetCurrentUserAsync());
                await _jobManager.Update(job);
                return ObjectMapper.Map<JobDto>(job);
            }
            catch (Exception ex) { throw new UserFriendlyException(L("JobUpdateFailed"), ex.Message); }

        }

        public async Task<JobDto> UpdateAllFields(JobUpdateInputDto input)
        {
            try
            {
                var job = await _jobManager.Get(input.Id);
                var user = await GetCurrentUserAsync();
                string imageCleanedDescription = await ExtractAndReplaceImagesInDescription(input.Description, job);

                // Update all fields using existing domain methods
                job = Job.SetTitle(job, input.Title, user);
                job = Job.SetDescription(job, imageCleanedDescription, user);
                job = Job.SetDueDate(job, input.DueDate ?? default, user);
                job = Job.SetLevel(job, input.Level, user);
                job = Job.SetParent(job, input.ParentId == Guid.Empty ? null : await _jobManager.Get(input.ParentId), user);
                Job.SetAssignee(job, input.AssigneeId == null ? null : await _userManager.GetUserByIdAsync(input.AssigneeId.Value), user);
                // Save changes once
                await _jobManager.Update(job);

                return ObjectMapper.Map<JobDto>(job);
            }
            catch (Exception ex) { throw new UserFriendlyException(L("JobUpdateFailed"), ex.Message); }

        }

        public async Task<RoadmapViewDto> GetRoadmapView(DateTime startDate, DateTime endDate)
        {
            var input = new GetAllJobsInput
            {
                MaxResultCount = int.MaxValue,
                Levels = new[] { JobLevel.Epic }
            };

            var jobs = await GetAll(input);
            
            // Include all epics that:
            // 1. Have start OR end date within range
            // 2. Have start date or due date at default value (will be placed at start of timeline)
            // 3. Span across the range (start before, end after)
            var filteredJobs = jobs.Items.Where(j => 
                // No dates set (both at default)
                (j.StartDate == default && j.DueDate == default) ||
                // Only start date set (due date at default) and it's in range
                (j.StartDate != default && j.DueDate == default && j.StartDate <= endDate) ||
                // Only due date set (start date at default) and it's in range
                (j.StartDate == default && j.DueDate != default && j.DueDate >= startDate) ||
                // Both dates set and either:
                // - Start date is in range
                // - End date is in range
                // - Dates span across the range
                (j.StartDate != default && j.DueDate != default && (
                    (j.StartDate <= endDate && j.StartDate >= startDate) ||
                    (j.DueDate >= startDate && j.DueDate <= endDate) ||
                    (j.StartDate <= startDate && j.DueDate >= endDate)
                ))
            ).ToList();

            return new RoadmapViewDto
            {
                StartDate = startDate,
                EndDate = endDate,
                Jobs = filteredJobs
            };
        }

        public async Task<JobDto> SetStartDate(Guid id, DateTime? startDate)
        {
            var job = await _jobManager.Get(id);
            var currentUser = await GetCurrentUserAsync();

            job = Job.SetStartDate(job, startDate, currentUser);
            await _jobManager.Update(job);

            return ObjectMapper.Map<JobDto>(job);
        }

        public async Task<JobDto> AddDependency(Guid jobId, Guid dependencyId)
        {
            var job = await _jobManager.Get(jobId);
            var dependency = await _jobManager.Get(dependencyId);
            var currentUser = await GetCurrentUserAsync();

            job = Job.AddDependency(job, dependency, currentUser);
            await _jobManager.Update(job);

            return ObjectMapper.Map<JobDto>(job);
        }

        public async Task<JobDto> RemoveDependency(Guid jobId, Guid dependencyId)
        {
            var job = await _jobManager.Get(jobId);
            var dependency = await _jobManager.Get(dependencyId);
            var currentUser = await GetCurrentUserAsync();

            job = Job.RemoveDependency(job, dependency, currentUser);
            await _jobManager.Update(job);

            return ObjectMapper.Map<JobDto>(job);
        }

        public async Task<JobDto> UpdateDates(Guid id, DateTime startDate, DateTime dueDate)
        {
            var job = await _jobManager.Get(id);
            var currentUser = await GetCurrentUserAsync();

            // Temporarily remove the validation to allow setting both dates
            job = Job.SetDates(job, startDate, dueDate, currentUser);
            await _jobManager.Update(job);

            return ObjectMapper.Map<JobDto>(job);
        }
    }
}
```

### 3. Infrastructure Layer

### 4. Presentation Layer

#### Controller
```csharp
using Microsoft.AspNetCore.Mvc;
using Abp.AspNetCore.Mvc.Authorization;
using toyiyo.todo.Controllers;
using toyiyo.todo.Authorization;
using toyiyo.todo.Jobs;
using System.Threading.Tasks;
using System;
using toyiyo.todo.Web.Models.Jobs;
using System.Collections.Generic;
using static toyiyo.todo.Jobs.Job;
using toyiyo.todo.Projects;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using toyiyo.todo.Web.Views.Shared.Components.UserDropdown;
using toyiyo.todo.Users;
using toyiyo.todo.Users.Dto;

namespace toyiyo.todo.Web.Controllers
{
    [AbpMvcAuthorize]
    public class JobsController : todoControllerBase
    {
        public JobsController(IJobAppService jobAppService, IProjectAppService projectAppService, IUserAppService userAppService)
        {
            JobAppService = jobAppService;
            ProjectAppService = projectAppService;
            UserAppService = userAppService;
        }

        public IJobAppService JobAppService { get; }
        public IProjectAppService ProjectAppService { get; }
        public IUserAppService UserAppService { get; }


        [HttpGet("/projects/{projectId}/jobs")]
        [HttpGet("/projects/{projectId}/jobs/{jobId}")]
        public async Task<IActionResult> Index(Guid projectId, Guid? jobId)
        {
            var project = await ProjectAppService.Get(projectId);
            ViewBag.ProjectId = projectId;
            ViewBag.JobId = jobId;
            ViewBag.ProjectTitle = project.Title;

            return View();
        }

        public async Task<IActionResult> EditModal(Guid JobId)
        {
            try
            {
                var output = await JobAppService.Get(JobId);
                if (output == null) { return new NotFoundResult(); }
                
                var subTasks = await JobAppService.GetAll(new GetAllJobsInput() { 
                    ParentJobId = JobId, 
                    MaxResultCount = int.MaxValue, 
                    Levels = new List<JobLevel> { JobLevel.SubTask}.ToArray() 
                });

                // Get all epics for the project
                var epics = await JobAppService.GetAll(new GetAllJobsInput() {
                    ProjectId = output.Project.Id,
                    MaxResultCount = int.MaxValue,
                    Levels = new List<JobLevel> { JobLevel.Epic }.ToArray(),
                    JobStatus = Status.Open
                });

                ViewBag.SubTasks = ObjectMapper.Map<List<EditJobSubTaskModalViewModel>>(subTasks.Items);
                ViewBag.Epics = epics.Items.Select(e => new SelectListItem { 
                    Value = e.Id.ToString(),
                    Text = e.Title,
                    Selected = e.Id == output.ParentId
                }).Prepend(new SelectListItem {
                    Value = Guid.Empty.ToString(),
                    Text = "No Parent",
                    Selected = !output.ParentId.HasValue
                });

                var model = ObjectMapper.Map<EditJobModalViewModel>(output);
                model.UserDropdown = new UserDropdownViewModel {
                    Users = (await UserAppService.GetAllAsync(new PagedUserResultRequestDto() { MaxResultCount = int.MaxValue })).Items.ToList(),
                    SelectedUserId = output.Assignee?.Id,
                    JobId = output.Id
                };
                return PartialView("_EditModal", model);
            }
            catch (ArgumentNullException) { return new NotFoundResult(); }
            catch (Abp.Domain.Entities.EntityNotFoundException) { return new NotFoundResult(); }
        }
    }
}
```

#### View
```cshtml
@using toyiyo.todo.Web.Startup
@{
    ViewBag.Title = L("Roadmap");
    ViewBag.CurrentPageName = PageNames.Roadmap;
}
@section styles {
    <link rel="stylesheet" href="//code.jquery.com/ui/1.13.2/themes/base/jquery-ui.css">
    <link rel="stylesheet" href="~/css/roadmap.css" asp-append-version="true">
}
@section scripts {
    <script src="https://code.jquery.com/ui/1.13.2/jquery-ui.min.js" 
            integrity="sha256-lSjKY0/srUM9BE3dPm+c4fBo1dky2v27Gdjm2uoZaL0=" 
            crossorigin="anonymous"></script>
    <environment names="Development">
        <script src="~/view-resources/Views/Jobs/Roadmap.js" asp-append-version="true"></script>
    </environment>
    <environment names="Staging,Production">
        <script src="~/view-resources/Views/Jobs/Roadmap.min.js" asp-append-version="true"></script>
    </environment>
}

<div class="content-header">
    <div class="container-fluid">
        <div class="row mb-2">
            <div class="col-sm-6">
                <h1 class="m-0">@L("Roadmap")</h1>
            </div>
        </div>
    </div>
</div>

<div class="content">
    <div class="container-fluid">
        <div class="card">
            <div class="card-header">
                <div class="row align-items-center">
                    <div class="col-md-6">
                        <div class="input-group">
                            <div class="input-group-prepend">
                                <span class="input-group-text">
                                    <i class="fas fa-calendar-week"></i>
                                </span>
                            </div>
                            <input type="date" class="form-control" id="startDate">
                            <div class="input-group-prepend input-group-append">
                                <span class="input-group-text">to</span>
                            </div>
                            <input type="date" class="form-control" id="endDate">
                        </div>
                    </div>
                </div>
            </div>
            <div class="card-body p-0">
                <div class="roadmap-wrapper">
                    <div id="roadmapContainer" class="roadmap-container"></div>
                </div>
            </div>
        </div>
    </div>
</div>

```

#### JavaScript
```javascript
(function ($) {
    var _projectService = abp.services.app.project,
        l = abp.localization.getSource('todo'),
        _$modal = $('#ProjectCreateModal'),
        _$deleteModal = $('#ProjectDeleteModal'),
        _$form = _$modal.find('form'),
        _$table = $('#ProjectsTable');

    var _$projectsTable = _$table.DataTable({
        paging: true,
        serverSide: true,
        listAction: {
            ajaxFunction: abp.services.app.project.getAll,
            inputFilter: function () {
                return {
                    keyword: $('#ProjectsSearchForm input[type=search]').val()
                }
            },
            dataFilter: function (data) {
                var json = jQuery.parseJSON(data);
                json.recordsTotal = json.totalCount;
                json.recordsFiltered = json.items.length;
                json.data = json.list;
                return JSON.stringify(json);
            }
        },
        buttons: [],
        responsive: {
            details: {
                type: 'column'
            }
        },
        columnDefs: [
            {
                targets: 0,
                data: null,
                className: 'project-card',
                render: function(data, type, row) {
                    const progress = row.progress;
                    const total = progress.totalJobCount;
                    const completed = progress.completedTasks;
                    const actions = DOMPurify.sanitize(`
                        <div class="dropdown">
                            <button class="btn btn-sm btn-light dropdown-toggle ml-2" type="button" data-toggle="dropdown" aria-expanded="false">
                                <i class="fas fa-ellipsis-v"></i>
                            </button>
                            <div class="dropdown-menu dropdown-menu-right">
                                <button type="button" class="dropdown-item edit-project" data-project-id="${row.id}" data-toggle="modal" data-target="#ProjectEditModal">
                                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-pencil mr-2" viewBox="0 0 16 16">
                                        <path d="M12.146.146a.5.5 0 0 1 .708 0l3 3a.5.5 0 0 1 0 .708l-10 10a.5.5 0 0 1-.168.11l-5 2a.5.5 0 0 1-.65-.65l2-5a.5.5 0 0 1 .11-.168l10-10zM11.207 2.5 13.5 4.793 14.793 3.5 12.5 1.207 11.207 2.5zm1.586 3L10.5 3.207 4 9.707V10h.5a.5.5 0 0 1 .5.5v.5h.5a.5.5 0 0 1 .5.5v.5h.293l6.5-6.5zm-9.761 5.175-.106.106-1.528 3.821 3.821-1.528.106-.106A.5.5 0 0 1 5 12.5V12h-.5a.5.5 0 0 1-.5-.5V11h-.5a.5.5 0 0 1-.468-.325z"/>
                                    </svg>
                                    ${l('Edit')}
                                </button>
                                <button type="button" class="dropdown-item delete-project" data-project-id="${row.id}" data-toggle="modal" data-target="#ProjectDeleteModal">
                                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-trash mr-2" viewBox="0 0 16 16">
                                        <path d="M5.5 5.5A.5.5 0 0 1 6 6v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5zm2.5 0a.5.5 0 0 1 .5.5v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5zm3 .5a.5.5 0 0 0-1 0v6a.5.5 0 0 0 1 0V6z"/>
                                        <path fill-rule="evenodd" d="M14.5 3a1 1 0 0 1-1 1H13v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V4h-.5a1 1 0 0 1-1-1V2a1 1 0 0 1 1-1H6a1 1 0 0 1 1 1h3.5a1 1 0 0 1 1 1v1zM4.118 4 4 4.059V13a1 1 0 0 0 1 1h6a1 1 0 0 0 1-1V4.059L11.882 4H4.118zM2.5 3V2h11v1h-11z"/>
                                    </svg>
                                    ${l('Delete')}
                                </button>
                            </div>
                        </div>
                    `);

                    // Generate card content regardless of task count
                    return `
                        <div class="card">
                            <div class="card-body">
                                <div class="d-flex justify-content-between align-items-center">
                                    <h5 class="card-title mb-0">
                                        <a class="project-title" href="/Projects/${DOMPurify.sanitize(row.id)}/jobs">${DOMPurify.sanitize(row.title)}</a>
                                    </h5>
                                    <div>
                                        <span class="badge badge-pill ${progress.statusClass}">${progress.status}</span>
                                        <span class="ml-2">${actions}</span>
                                    </div>
                                </div>
                                <div class="text-muted mt-3">
                                    <div class="project-stats">
                                        <span class="stat-item">
                                            <i class="fas fa-layer-group mr-2"></i>
                                            ${progress.epicCount} Epics (${progress.completedEpics} done)
                                        </span>
                                        <span class="stat-item">
                                            <i class="fas fa-tasks mr-2"></i>
                                            ${progress.taskCount} Tasks 
                                        </span>
                                        <span class="stat-item">
                                            <i class="fas fa-bug mr-2"></i>
                                            ${progress.bugCount} Bugs
                                        </span>
                                        ${progress.dueDate && !moment(progress.dueDate).isSame('0001-01-01T00:00:00Z') ? `
                                            <span class="stat-item">
                                                <i class="fas fa-calendar mr-2"></i>
                                                Due: ${moment(progress.dueDate).format('M/D/YYYY')}
                                            </span>
                                        ` : `
                                            <span class="stat-item text-muted">
                                                <i class="fas fa-calendar mr-2"></i>
                                                No due date set
                                            </span>
                                        `}
                                    </div>
                                </div>
                            </div>
                            <div class="card-footer p-2">
                                <div class="progress position-relative" style="height: 20px;">
                                    <div class="progress-bar bg-success" 
                                         style="width: ${progress.totalTasksPercentage}%"
                                         role="progressbar" 
                                         aria-valuenow="${progress.totalTasksPercentage}" 
                                         aria-valuemin="0" 
                                         aria-valuemax="100"
                                         data-toggle="tooltip"
                                         title="${completed} completed (${progress.totalTasksPercentage}%)">
                                    </div>
                                    <div class="position-absolute w-100 text-center" style="line-height: 20px;">
                                        ${total === 0 ? 'No tasks' : `${completed} of ${total} tasks completed (${progress.totalTasksPercentage}%)`}
                                    </div>
                                </div>
                            </div>
                        </div>
                    `;
                }
            }
        ]
    });

    _$projectsTable.on('draw.dt', function() {
        $('[data-toggle="tooltip"]').tooltip();
    });

    _$form.find('.save-button').on('click', (e) => {
        e.preventDefault();

        if (!_$form.valid()) {
            return;
        }

        var project = _$form.serializeFormToObject();

        abp.ui.setBusy(_$modal);
        _projectService
            .create(project)
            .done(function () {
                _$modal.modal('hide');
                _$form[0].reset();
                abp.notify.info(l('Saved Successfully'));
                _$projectsTable.ajax.reload();
            })
            .always(function () {
                abp.ui.clearBusy(_$modal);
            });
    });


    $(document).on('click', '.edit-project', function (e) {
        var projectId = $(this).attr("data-project-id");

        e.preventDefault();
        abp.ajax({
            url: abp.appPath + 'Projects/EditModal?projectId=' + projectId,
            type: 'POST',
            dataType: 'html',
            success: function (content) {
                $('#ProjectEditModal div.modal-content').html(content);
            }
        })
    });

    abp.event.on('project.edited', () => {
        _$projectsTable.ajax.reload();
    });

    _$modal.on('shown.bs.modal', () => {
        _$modal.find('input:not([type=hidden]):first').focus();
    }).on('hidden.bs.modal', () => {
        _$form.clearForm();
    });

    $('.btn-search').on('click', () => {
        _$projectsTable.ajax.reload();
    });

    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            _$projectsTable.ajax.reload();
            return false;
        }
    });

    //triggered when the delete modal - sets the project id to be deleted
    _$deleteModal.on('show.bs.modal', function (e) {

        //get data-id attribute of the clicked element
        let projectId = $(e.relatedTarget).attr('data-project-id');

        //populate the textbox
        $(e.currentTarget).find('input[name="ProjectId"]').val(projectId);
    });

    _$deleteModal.on('click', '.delete-button', function (e) {
        let projectId = $(this).closest('div.modal-content').find('input[name="ProjectId"]').val();

        e.preventDefault();

        abp.ui.setBusy(_$deleteModal);

        _projectService
            .delete(projectId)
            .done(function () {
                abp.notify.info(l('Deleted Successfully'));
                _$projectsTable.ajax.reload();
            })
            .always(function () {
                abp.ui.clearBusy(_$deleteModal);
            });
    });

})(jQuery);

```

## Key Architecture Rules

1. Domain Layer Rules
   - All business logic lives here
   - Rich domain model with protected setters
   - Factory methods for validation
   - Domain events for decoupling
   - No dependencies on outer layers

2. Application Layer Rules
   - Thin orchestration layer
   - Uses domain managers
   - DTOs for I/O
   - Handles transactions
   - No domain logic

3. Infrastructure Layer Rules
   - Implements domain interfaces
   - Handles technical concerns
   - No domain logic
   - Manages external services

4. Presentation Layer Rules
   - Controllers are thin
   - Delegate to application services
   - Handle only HTTP/UI concerns
   - Client-side validation

# Key Features and Best Practices

- **Rich Domain Model**: Business rules are encapsulated in the domain layer
- **Protected Setters**: Forces use of domain methods for state changes
- **Domain Events**: For loose coupling between domain operations
- **Repository Pattern**: Abstracts data access behind interfaces
- **Unit of Work**: Ensures transactional consistency
- **Application Services**: Thin layer coordinating domain operations
- **DTOs**: Clean separation between layers
- **Validation**: Both domain and application level validation

# Coding Standards

1. Domain entities should protect their invariants:
   - Use protected setters
   - Validate in static factory methods
   - Throw domain exceptions for rule violations

2. Application services should:
   - Use DTOs for input/output
   - Not contain domain logic
   - Handle transaction boundaries
   - Map between DTOs and domain objects

3. Infrastructure should:
   - Implement interfaces defined in domain
   - Handle technical concerns
   - Not leak to upper layers

4. Controllers should:
   - Be thin
   - Only handle HTTP concerns
   - Delegate to application services

# Frontend Configuration

## Bundle Configuration
The application uses libman for client-side library management and a custom bundling system for optimizing frontend assets.

### Key Libraries
- Tribute.js: For @mentions functionality
- Marked: For markdown rendering
- SignalR: For real-time notifications
- JQuery UI: For UI interactions
- DataTables: For interactive tables

### Bundle Structure
```json
{
  "shared-layout.min.css": [
    "libs/font-awesome/css/all.min.css",
    "libs/tributejs/tribute.css",
    "libs/admin-lte/dist/css/adminlte.min.css",
    "libs/datatables/*.css",
    "css/style.css"
  ],
  "shared-layout.min.js": [
    "libs/jquery/jquery.js",
    "libs/jquery-ui/jquery-ui.min.js",
    "libs/bootstrap/dist/js/bootstrap.bundle.js",
    "libs/datatables/*.js",
    "libs/tributejs/tribute.min.js",
    "libs/marked/marked.min.js",
    "libs/signalr/*.js",
    "js/main.js"
  ]
}
```

### Library Management Rules
1. All third-party libraries should be managed through libman.json
2. CDN dependencies should be avoided - use local files for reliability
3. Bundle configurations should be maintained in bundleconfig.json
4. Development environment uses individual files, production uses minified bundles

### Script Loading Order
1. Core dependencies (jQuery, Bootstrap)
2. UI libraries (AdminLTE, DataTables)
3. Feature libraries (Tribute.js, Marked)
4. Application-specific code

### Notification System Integration
```javascript
abp.notifications.messageFormatters['toyiyo.todo.Notifications.NoteMentionNotificationData'] = 
  function (userNotification) {
    var data = userNotification.notification.data;
    return abp.localization.localize(
      'UserMentionedNotification',
      'todo',
      [data.senderUsername, data.jobTitle]
    );
};
```

### Notification System Architecture

The application uses ASP.NET Boilerplate's notification system with the following components:

1. **Notification Definition**
```csharp
public class TodoNotificationProvider : NotificationProvider
{
    public override void SetNotifications(INotificationDefinitionContext context)
    {
        context.Manager.Add(
            new NotificationDefinition(
                "Note.Mention",
                displayName: new LocalizableString("NoteMentionNotificationDefinition", "todo"),
                permissionDependency: null
            )
        );
    }
}
```

2. **Notification Data Types**
```csharp
[Serializable]
public class NoteMentionNotificationData : NotificationData
{
    public string Message { get; set; }
    public string JobTitle { get; set; }
    public string SenderUsername { get; set; }

    public NoteMentionNotificationData(string message, string jobTitle, string senderUsername)
    {
        Message = message;
        JobTitle = jobTitle;
        SenderUsername = senderUsername;
    }
}
```

3. **Publishing Notifications**
```csharp
public async Task NotifyMention(string message, string jobTitle, UserIdentifier targetUserId)
{
    await _notificationPublisher.PublishAsync(
        "Note.Mention",
        new NoteMentionNotificationData(message, jobTitle, AbpSession.GetUserName()),
        userIds: new[] { targetUserId }
    );
}
```

4. **Client-Side Integration**
```javascript
// Register notification formatter
abp.notifications.messageFormatters['toyiyo.todo.Notifications.NotificationData.NoteMentionNotificationData'] = 
    function (userNotification) {
        var data = userNotification.notification.data;
        var message = data.properties ? data.properties.Message : null;
        return message || 'You have been mentioned in a note';
    };

// Listen for notifications
abp.event.on('abp.notifications.received', function (userNotification) {
    abp.notifications.showUiNotifyForUserNotification(userNotification);
});
```

### Key Notification Rules

1. **Notification Types**:
   - Define clear notification names (e.g., "Note.Mention")
   - Use proper notification data classes
   - Register notification definitions in the module initializer

2. **Client Integration**:
   - Register formatters for each notification type
   - Use consistent naming across backend and frontend
   - Handle notification data structure correctly

3. **Data Structure**:
```javascript
// Example notification object structure
{
    "userId": 2,
    "state": 0,
    "notification": {
        "notificationName": "Note.Mention",
        "data": {
            "properties": {
                "Message": "@username mentioned you",
                "JobTitle": "Task title",
                "SenderUsername": "sender"
            },
            "type": "toyiyo.todo.Notifications.NotificationData.NoteMentionNotificationData"
        },
        "severity": 0,
        "creationTime": "2024-02-21T15:06:58.917627Z"
    }
}
```

# Testing Guidelines

## Test Project Setup
1. Create a new Class Library project under `aspnet-core/test` directory.
2. Add the following NuGet packages:
   - `Abp.TestBase`: Provides base classes for testing ABP based projects.
   - `Abp.EntityFrameworkCore`: For Entity Framework Core integration.
   - `Effort.EFCore`: For creating an in-memory database.
   - `xunit`: The testing framework.
   - `xunit.runner.visualstudio`: To run tests in Visual Studio.
   - `Shouldly`: For easy-to-read assertions.

## Base Test Class
Create a base test class to initialize the ABP system and set up an in-memory database:
```csharp
public abstract class TodoTestBase : AbpIntegratedTestBase<TodoTestModule>
{
    protected TodoTestBase()
    {
        UsingDbContext(context => new TodoInitialDataBuilder().Build(context));
    }

    protected override void PreInitialize()
    {
        LocalIocManager.IocContainer.Register(
            Component.For<DbConnection>()
                .UsingFactoryMethod(Effort.DbConnectionFactory.CreateTransient)
                .LifestyleSingleton()
        );

        base.PreInitialize();
    }

    public void UsingDbContext(Action<TodoDbContext> action)
    {
        using (var context = LocalIocManager.Resolve<TodoDbContext>())
        {
            context.DisableAllFilters();
            action(context);
            context.SaveChanges();
        }
    }

    public T UsingDbContext<T>(Func<TodoDbContext, T> func)
    {
        T result;

        using (var context = LocalIocManager.Resolve<TodoDbContext>())
        {
            context.DisableAllFilters();
            result = func(context);
            context.SaveChanges();
        }

        return result;
    }
}
```

## Test Module
Create a test module to define dependencies:
```csharp
[DependsOn(
    typeof(TodoDataModule),
    typeof(TodoApplicationModule),
    typeof(AbpTestBaseModule)
)]
public class TodoTestModule : AbpModule
{
}
```

## Initial Data Builder
Create an initial data builder to seed the database:
```csharp
public class TodoInitialDataBuilder
{
    public void Build(TodoDbContext context)
    {
        context.Users.AddOrUpdate(
            u => u.UserName,
            new User { UserName = "admin" },
            new User { UserName = "user1" }
        );
        context.SaveChanges();

        context.Projects.AddOrUpdate(
            p => p.Name,
            new Project { Name = "Project 1" },
            new Project { Name = "Project 2" }
        );
        context.SaveChanges();
    }
}
```

## Example Test
Create a test class to test application services:
```csharp
public class JobAppService_Tests : TodoTestBase
{
    private readonly IJobAppService _jobAppService;

    public JobAppService_Tests()
    {
        _jobAppService = LocalIocManager.Resolve<IJobAppService>();
    }

    [Fact]
    public void Should_Create_New_Jobs()
    {
        var initialJobCount = UsingDbContext(context => context.Jobs.Count());
        var project = UsingDbContext(context => context.Projects.First());

        _jobAppService.Create(new JobCreateInputDto
        {
            ProjectId = project.Id,
            Title = "New Job",
            Description = "Job Description"
        });

        UsingDbContext(context =>
        {
            context.Jobs.Count().ShouldBe(initialJobCount + 1);
            context.Jobs.FirstOrDefault(j => j.Title == "New Job").ShouldNotBeNull();
        });
    }

    [Fact]
    public async Task Should_Do_Something()
    {
        // Arrange
        var currentUser = await GetCurrentUserAsync();
        var currentTenant = await GetCurrentTenantAsync();
        
        var project = await _projectAppService.Create(new CreateProjectInputDto 
        { 
            Title = "test project" 
        });
        
        var job = await _jobAppService.Create(new JobCreateInputDto 
        { 
            ProjectId = project.Id, 
            Title = "test job", 
            Description = "test job" 
        });

        // Act
        var result = await _myService.DoSomething(job.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Status.ShouldBe(expectedStatus);
    }

    [Fact]
    public async Task Should_Assign_Job_To_User()
    {
        // Arrange
        var project = await _projectAppService.Create(new CreateProjectInputDto 
        { 
            Title = "test project" 
        });
        
        var job = await _jobAppService.Create(new JobCreateInputDto 
        { 
            ProjectId = project.Id, 
            Title = "test job" 
        });

        var assigneeUser = await GetUserByUserNameAsync("username");

        // Act
        await _jobAppService.AssignJob(new AssignJobInputDto
        {
            JobId = job.Id,
            UserId = assigneeUser.Id
        });

        // Assert
        var updatedJob = await _jobAppService.Get(job.Id);
        updatedJob.AssigneeId.ShouldBe(assigneeUser.Id);
    }

    [Fact]
    public async Task Should_Change_Job_Status()
    {
        // Arrange
        var project = await _projectAppService.Create(new CreateProjectInputDto 
        { 
            Title = "test" 
        });
        
        var job = await _jobAppService.Create(new JobCreateInputDto 
        { 
            ProjectId = project.Id, 
            Title = "test job" 
        });

        // Act
        await _jobAppService.SetJobStatus(new JobSetStatusInputDto
        {
            Id = job.Id,
            JobStatus = Status.Done
        });

        // Assert
        var updatedJob = await _jobAppService.Get(job.Id);
        updatedJob.Status.ShouldBe(Status.Done);
    }
}
```

## Running Tests
1. Open Visual Studio Test Explorer by selecting `TEST\Windows\Test Explorer`.
2. Click 'Run All' to execute all tests in the solution.

## Best Practices
- Use `UsingDbContext` methods to interact with the database.
- Follow the Arrange-Act-Assert pattern in test methods.
- Use `Shouldly` for assertions to improve readability.
- Ensure tests are isolated and do not depend on each other
- Never use new Job() or new Project() directly
- Always use application services to create entities
- Use GetCurrentUserAsync() and GetCurrentTenantAsync()
- Login as tenant admin in constructor
- Clean up test data in Dispose if needed
- Use meaningful test data names (e.g., "test project", "test job")
- Include both positive and negative test cases
- Test authorization rules
- Use Shouldly for assertions
- Follow Arrange-Act-Assert pattern
- Common Pitfalls to Avoid

- Don't create entities with new keyword
- Don't manipulate protected setters
- Don't bypass application services
- Don't create users manually
- Don't skip tenant context
- Don't use hardcoded IDs