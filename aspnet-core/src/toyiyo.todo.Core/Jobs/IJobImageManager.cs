using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using toyiyo.todo.Authorization.Users;

namespace toyiyo.todo.Jobs
{
    public interface IJobImageManager
    {
        Task<JobImage> Get(Guid id);
        Task<JobImage> Create(JobImage jobImage);
        Task Delete(Guid id, User user);
        Task<List<JobImage>> GetByJobId(Guid jobId);
        Task<JobImage> GetByHash(string contentHash);
    }
}