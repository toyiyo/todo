using System;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Domain.Services;
using toyiyo.todo.Authorization.Users;
using System.Collections.Generic;

namespace toyiyo.todo.Jobs
{

    public class JobImageManager : DomainService, IJobImageManager
    {
        private readonly IRepository<JobImage, Guid> _jobImageRepository;

        public JobImageManager(IRepository<JobImage, Guid> jobImageRepository)
        {
            _jobImageRepository = jobImageRepository;
        }

        public async Task<JobImage> Get(Guid id)
        {
            return await _jobImageRepository.GetAsync(id);
        }

        public async Task<JobImage> Create(JobImage jobImage)
        {
            return await _jobImageRepository.InsertAsync(jobImage);
        }

        public async Task Delete(Guid id, User user)
        {
            await _jobImageRepository.DeleteAsync(id);
        }

        public async Task<List<JobImage>> GetByJobId(Guid jobId)
        {
            return await _jobImageRepository.GetAllListAsync(x => x.JobId == jobId);
        }
    }
}