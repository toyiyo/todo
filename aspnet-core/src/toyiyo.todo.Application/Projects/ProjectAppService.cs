using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using toyiyo.todo.Projects.Dto;

namespace toyiyo.todo.Projects {
    public class ProjectAppService : todoAppServiceBase, IProjectAppService {
        private readonly IProjectManager _projectManager;
        public ProjectAppService(IProjectManager projectManager) {
            _projectManager = projectManager;
        }
        /// <summary> Creates a new Project. </summary>
        public async Task<ProjectDto> Create(ProjectDto input) {
            var tenant = await GetCurrentTenantAsync();
            var project = Project.Create(input.Title, await GetCurrentUserAsync(), tenant.Id);
            await _projectManager.Create(project);
            return ObjectMapper.Map<ProjectDto>(project);
        }
        /// <summary> Gets a Project by Id. </summary>
        public async Task<ProjectDto> Get(Guid id) {
            var project = await _projectManager.Get(id);
            return ObjectMapper.Map<ProjectDto>(project);
        }
        /// <summary> Gets all Projects. Keyword filters by Title</summary>
        public async Task<PagedResultDto<ProjectDto>> GetAll(GetAllProjectsInput input) {
            var projects = await _projectManager.GetAll(input);
            return new PagedResultDto<ProjectDto>(projects.Count, ObjectMapper.Map<List<ProjectDto>>(projects));
        }
    }
}