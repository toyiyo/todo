using System;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using toyiyo.todo.Projects.Dto;

namespace toyiyo.todo.Projects
{
    public interface IProjectAppService
    {
        Task<ProjectDto> Create(CreateProjectInputDto input);
        Task<ProjectDto> Get(Guid id);
        Task<PagedResultDto<ProjectDto>> GetAll(GetAllProjectsInput input);
        Task<ProjectDto> SetTitle(ProjectDto input);
    }
}