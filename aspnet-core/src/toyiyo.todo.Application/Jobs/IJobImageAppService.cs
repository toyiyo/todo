
using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace toyiyo.todo.Jobs
{
    public interface IJobImageAppService : IApplicationService
    {
        Task<JobImageDto> Create(JobImageCreateInputDto input);
        Task<JobImageDto> Get(Guid id);
        Task<IActionResult> GetImage(Guid id);
        Task<IActionResult> Delete(Guid id);
    }
}