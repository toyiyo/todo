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
            var job = Job.Create(project, input.Title, input.Description, await GetCurrentUserAsync(), tenant.Id, input.DueDate ?? default, input.ParentId ?? default, input.Level);
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
            string imageCleanedDescription = await ExtractAndReplaceImagesInDescription(input.Description, currentUser, job);
            job = Job.SetDescription(job, imageCleanedDescription, currentUser);
            await _jobManager.Update(job);

            return ObjectMapper.Map<JobDto>(job);
        }

        private async Task<string> ExtractAndReplaceImagesInDescription(string description, User currentUser, Job job)
        {
            var images = _imageExtractor.ExtractImages(description);
            var imageIdMap = new Dictionary<string, string>();

            foreach (var img in images)
            {
                var imageData = Convert.FromBase64String(img.Base64Data);
                var contentHash = JobImage.ComputeHash(imageData);

                // Check if image already exists
                var existingImage = await _jobImageManager.GetByHash(contentHash);
                if (existingImage != null)
                {
                    imageIdMap.TryAdd(img.Base64Data, existingImage.ImageUrl);
                }
                else
                {                    
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
                string imageCleanedDescription = await ExtractAndReplaceImagesInDescription(input.Description, user, job);

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
    }
}