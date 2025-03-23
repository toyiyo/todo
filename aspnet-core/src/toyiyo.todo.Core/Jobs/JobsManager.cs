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
using toyiyo.todo.Authorization.Users;

namespace toyiyo.todo.Jobs
{
    public class JobManager : DomainService, IJobManager
    {
        public readonly IRepository<Job, Guid> _jobRepository;
        public JobManager(IRepository<Job, Guid> jobRepository)
        {
            _jobRepository = jobRepository;
        }

        public async Task<Job> Get(Guid id)
        {
            return await _jobRepository.Query(q => q.Where(p => p.Id == id)
                        .Include(p => p.Project)
                        .Include(p => p.Assignee)
                        .Include(p => p.Owner)
                        .FirstOrDefaultAsync());

        }
        //GetAll() repository method requires a unit of work to be open. see https://aspnetboilerplate.com/Pages/Documents/Unit-Of-Work#irepository-getall-method
        [UnitOfWork]
        public async Task<List<Job>> GetAll(GetAllJobsInput input)
        {
            return await GetAllJobsQueryable(input)
            .Include(p => p.Project)
            .Include(p => p.Assignee)
            .Include(p => p.Owner)
            .OrderBy<Job>(input?.Sorting ?? "OrderByDate DESC")
            .Skip(input?.SkipCount ?? 0)
            .Take(input?.MaxResultCount ?? int.MaxValue)
            .ToListAsync();
        }

        [UnitOfWork]
        public async Task<int> GetAllCount(GetAllJobsInput input)
        {
            return await GetAllJobsQueryable(input).CountAsync();
        }

        private IQueryable<Job> GetAllJobsQueryable(GetAllJobsInput input)
        {
            //repository methods already filter by tenant, we can check other attributes by adding "or" "||" to the whereif clause
            var query = _jobRepository.GetAll()
                .WhereIf(!input.ProjectId.Equals(Guid.Empty), x => x.Project.Id == input.ProjectId)
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), p => p.Title.ToUpper().Contains(input.Keyword.ToUpper()) || p.Description.ToUpper().Contains(input.Keyword.ToUpper()))
                .WhereIf(input.JobStatus != null, p => p.JobStatus == input.JobStatus)
                .WhereIf(!input.ParentJobId.Equals(Guid.Empty), p => p.ParentId == input.ParentJobId)
                .WhereIf(input.Levels != null && input.Levels.Any(), p => input.Levels.Contains(p.Level));

            return query;
        }

        [UnitOfWork]
        public async Task<Job> Create(Job inputJob)
        {
            var job = await _jobRepository.InsertAsync(inputJob);
            return job;
        }

        [UnitOfWork]
        public async Task<Job> Update(Job inputJob)
        {
            var job = await _jobRepository.UpdateAsync(inputJob);
            return job;
        }

        public async Task Delete(Guid id, User user)
        {
            var job = Job.Delete(await this.Get(id), user);
            await _jobRepository.DeleteAsync(job);
        }

        public async Task<Job> SetOrderByDate(Guid id, User user, DateTime orderByDate)
        {
            //the manager is ust a pass through to instantiate the correct class to handle the logic
            //For our domain, the Job class contains the domain logic
            var job = Job.SetOrderByDate(await Get(id), user, orderByDate);
            //Our repository class already filters by tenant, so when getting the job by id, if the current user doesn't have access, the job will be empty and no update will happen
            return await _jobRepository.UpdateAsync(job);
        }
    }
}