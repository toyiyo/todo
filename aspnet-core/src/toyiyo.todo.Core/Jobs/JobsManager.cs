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
            var job = await _jobRepository.GetAsync(id);
            return job;
        }
        //GetAll() repository method requires a unit of work to be open. see https://aspnetboilerplate.com/Pages/Documents/Unit-Of-Work#irepository-getall-method
        [UnitOfWork]
        public async Task<List<Job>> GetAll(GetAllJobsInput input)
        {
            //repository methods already filter by tenant, we can check other attributes by adding "or" "||" to the whereif clause
            //todo: filter by JobStatus
            return await _jobRepository.GetAll()
            .WhereIf(!input.ProjectId.Equals(Guid.Empty), x => x.Project.Id == input.ProjectId)
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), p => p.Title.ToUpper().Contains(input.Keyword.ToUpper()))
            .WhereIf(!(input.JobStatus == null), p => p.JobStatus == input.JobStatus)
            .Include(p => p.Project)
            .Include(p => p.Assignee)
            //.Include(p => p.Members)
            .Include(p => p.Owner)
            .OrderBy<Job>(input?.Sorting ?? "CreationTime DESC")
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToListAsync();
        }

        public async Task<Job> Create(Job inputJob)
        {
            var job = await _jobRepository.InsertAsync(inputJob);
            return job;
        }

        public async Task<Job> Update(Job inputJob)
        {
            var job = await _jobRepository.UpdateAsync(inputJob);
            return job;
        }
    }
}