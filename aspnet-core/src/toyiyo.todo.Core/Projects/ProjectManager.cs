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
        public async Task<List<Project>> GetAll(GetAllProjectsInput input)
        {
            //repository methods already filter by tenant, we can check other attributes by adding "or" "||" to the whereif clause
            //todo: figure out how to ignore case when searching for title in postgresql
            return await GetAllProjectsQueryable(input)
            .OrderBy<Project>(input?.Sorting ?? "CreationTime DESC")
            .Skip(input?.SkipCount ?? 0)
            .Take(input?.MaxResultCount ?? int.MaxValue)
            .ToListAsync();
        }

        [UnitOfWork]
        public async Task<int> GetAllCount(GetAllProjectsInput input)
        {
            return await GetAllProjectsQueryable(input).CountAsync();
        }

        private IQueryable<Project> GetAllProjectsQueryable(GetAllProjectsInput input)
        {
            //repository methods already filter by tenant, we can check other attributes by adding "or" "||" to the whereif clause
            return _projectRepository.GetAll()
            .WhereIf(!input.keyword.IsNullOrWhiteSpace(), p => p.Title.ToUpper().Contains(input.keyword.ToUpper()));
        }

        public async Task<Project> Create(Project inputProject)
        {
            var project = await _projectRepository.InsertAsync(inputProject);
            return project;
        }

        public async Task<Project> Update(Project inputProject)
        {
            var project = await _projectRepository.UpdateAsync(inputProject);
            return project;
        }

        public async Task Delete(Project inputProject)
        {
            //in theory, this will soft delete the project
            //https://aspnetboilerplate.com/Pages/Documents/Entities?searchKey=soft%20delete
            await _projectRepository.DeleteAsync(inputProject.Id);
        }
    }
}