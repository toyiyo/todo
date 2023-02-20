using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Microsoft.AspNetCore.Mvc;
using toyiyo.todo.Authorization;
using toyiyo.todo.Jobs.Dto;
using toyiyo.todo.Projects;

namespace toyiyo.todo.Jobs
{
    [AbpAuthorize(PermissionNames.Pages_Jobs)]
    public class JobAppService : todoAppServiceBase, IJobAppService
    {
        private readonly IJobManager _jobManager;
        private readonly IProjectManager _projectManager;
        public JobAppService(IJobManager jobManager, IProjectManager projectManager)
        {
            _projectManager = projectManager;
            _jobManager = jobManager;
        }
        /// <summary> Creates a new Job. </summary>
        public async Task<JobDto> Create(JobCreateInputDto input)
        {
            var tenant = await GetCurrentTenantAsync();
            var project = await _projectManager.Get(input.ProjectId);
            var job = Job.Create(project, input.Title, input.Description, await GetCurrentUserAsync(), tenant.Id, input.DueDate ?? default );
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

        public async Task<JobStatsDto> GetJobStats(GetAllJobsInput getAllJobsInput)
        {
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

        public async Task<JobDto> SetDescription(JobSetDescriptionInputDto jobSetDescriptionInputDto)
        {
            var job = Job.SetDescription(await _jobManager.Get(jobSetDescriptionInputDto.Id), jobSetDescriptionInputDto.Description, await GetCurrentUserAsync());
            await _jobManager.Update(job);
            return ObjectMapper.Map<JobDto>(job);
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
    }
}