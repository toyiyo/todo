using System;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Microsoft.AspNetCore.Mvc;
using toyiyo.todo.Jobs.Dto;

namespace toyiyo.todo.Jobs
{
    public interface IJobAppService
    {
        Task<JobDto> Create(JobCreateInputDto input);
        Task<JobDto> Get(Guid id);
        Task<PagedResultDto<JobDto>> GetAll(GetAllJobsInput input);
        Task<JobDto> SetTitle(JobSetTitleInputDto input);
        Task<JobDto> SetDescription(JobSetDescriptionInputDto jobSetDescriptionInputDto);
        Task<JobDto> SetJobStatus(JobSetStatusInputDto jobSetStatusInputDto);
        Task<JobDto> SetDueDate(JobSetDueDateInputDto jobSetDueDateInputDto);
        Task<JobDto> SetParent(JobSetParentInputDto jobSetParentInputDto);
        Task<IActionResult> Delete(Guid id);
        Task<ActionResult<JobDto>> PatchOrderByDate(JobPatchOrderByDateInputDto jobPatchOrderByDateInputDto);
        Task<JobStatsDto> GetJobStats();
        Task<JobDto> SetLevel(JobSetLevelInputDto input);
        Task<JobDto> SetAssignee(JobSetAssigneeInputDto input);
        Task<JobDto> UpdateAllFields(JobUpdateInputDto input);
        Task<RoadmapViewDto> GetRoadmapView(DateTime startDate, DateTime endDate);
        Task<JobDto> SetStartDate(Guid id, DateTime? startDate);
        Task<JobDto> AddDependency(Guid jobId, Guid dependencyId);
        Task<JobDto> RemoveDependency(Guid jobId, Guid dependencyId);
        Task<JobDto> UpdateDates(Guid id, DateTime startDate, DateTime dueDate);

    }
}