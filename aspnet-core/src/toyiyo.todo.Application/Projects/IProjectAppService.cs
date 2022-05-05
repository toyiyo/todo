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
        //create(title, projectId)
//archive(jobid)
//comment(comment)
//prioritize(priority)
//setTitle(title)
//setDescription(description)
//setDueDate(dueDate)
//setOwner(owner)
//assign(user)
//unassign()
//setStatus(status)
//inviteMembers(members)
//removeMembers(members)
//moveToProject(projectId)

//getJob(id)
//getJobs(filter)
//getHistoryOnJob(id)
    }
}