using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace toyiyo.todo.Projects
{
    internal interface IProjectManager
    {
        Task<Project> Create(Project inputProject);
        Task<Project> Get(Guid id);
        Task<List<Project>> GetAll(GetAllProjectsInput input);
    }
}