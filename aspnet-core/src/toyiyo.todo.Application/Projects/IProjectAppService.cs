using System;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using toyiyo.todo.Projects.Dto;

namespace toyiyo.todo.Projects
{
    internal interface IProjectAppService
    {
        Task<ProjectDto> Create(ProjectDto input);
        Task<ProjectDto> Get(Guid id);
        Task<PagedResultDto<ProjectDto>> GetAll(GetAllProjectsInput input);
    }
}