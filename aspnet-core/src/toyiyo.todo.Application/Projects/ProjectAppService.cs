using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using toyiyo.todo.Authorization;
using toyiyo.todo.Projects.Dto;

namespace toyiyo.todo.Projects {
    [AbpAuthorize(PermissionNames.Pages_Projects)]
    public class ProjectAppService : todoAppServiceBase, IProjectAppService {
        private readonly IProjectManager _projectManager;
        public ProjectAppService(IProjectManager projectManager) {
            _projectManager = projectManager;
        }
        /// <summary> Creates a new Project. </summary>
        public async Task<ProjectDto> Create(CreateProjectInputDto input) {
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
            
            // Get progress for all projects in one batch
            var progressMap = await _projectManager.GetProjectsProgress(
                projects.Select(p => p.Id)
            );
            
            var dtos = projects.Select(p => {
                var dto = ObjectMapper.Map<ProjectDto>(p);
                dto.Progress = ObjectMapper.Map<ProjectProgressDto>(progressMap[p.Id]);
                return dto;
            }).ToList();
            
            var projectsTotalCount = await _projectManager.GetAllCount(input);
            return new PagedResultDto<ProjectDto>(projectsTotalCount, dtos);
        }

        /// <summary> Sets the project's title </summary>
        public async Task<ProjectDto> SetTitle(ProjectDto input) {
            var project = Project.SetTitle(await _projectManager.Get(input.Id), input.Title, await GetCurrentUserAsync());
            await _projectManager.Update(project);
            return ObjectMapper.Map<ProjectDto>(project);
        }

        /// <summary> Archives a project by setting is as soft deleted </summary>
        public async Task Delete(Guid id) {
            var project = Project.Delete(await _projectManager.Get(id), await GetCurrentUserAsync());
            await _projectManager.Delete(project);
        }
    }
}