using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;

namespace toyiyo.todo.Projects
{
    public class ProjectManager : DomainService, IProjectManager
    {
        public readonly IRepository<Project, Guid> _projectRepository;
        public ProjectManager(IRepository<Project, Guid> projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<Project> Get(Guid id)
        {
            var project = await _projectRepository.GetAsync(id);
            return project;
        }
        //GetAll() repository method requires a unit of work to be open. see https://aspnetboilerplate.com/Pages/Documents/Unit-Of-Work#irepository-getall-method
        [UnitOfWork]
        public async Task<List<Project>> GetAll(GetAllProjectsInput input) {
            //repository methods already filter by tenant, we can check other attributes by adding "or" "||" to the whereif clause
            return await _projectRepository.GetAll()
            .WhereIf(!input.keyword.IsNullOrWhiteSpace(), p => p.Title.Contains(input.keyword))
            .OrderBy<Project>(input.Sorting)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToListAsync();
        }

        public async Task<Project> Create(Project inputProject)
        {
            var project = await _projectRepository.InsertAsync(inputProject);
            return project;
        }
    }
}