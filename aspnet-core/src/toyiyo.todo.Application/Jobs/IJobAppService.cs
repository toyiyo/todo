using System;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
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
        Task<JobDto> SetStatus(JobSetStatusInputDto jobSetStatusInputDto);
        Task<JobDto> SetDueDate(JobSetDueDateInputDto jobSetDueDateInputDto);
    }
}