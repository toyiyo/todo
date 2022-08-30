using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace toyiyo.todo.Projects
{
    public interface IProjectManager
    {
        Task<Project> Create(Project inputProject);
        Task<Project> Get(Guid id);
        Task<List<Project>> GetAll(GetAllProjectsInput input);
        Task<int> GetAllCount(GetAllProjectsInput input);
        Task<Project> Update(Project inputProject);
        Task Delete(Project inputProject);
    }
}