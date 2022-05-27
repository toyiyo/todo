using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
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
            var job = Job.Create(project, input.Title, input.Description, await GetCurrentUserAsync(), tenant.Id);
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
            var job = Job.SetDueDate(await _jobManager.Get(jobSetDueDateInputDto.Id), jobSetDueDateInputDto.DueDate, await GetCurrentUserAsync());
            await _jobManager.Update(job);
            return ObjectMapper.Map<JobDto>(job);
        }
    }
}