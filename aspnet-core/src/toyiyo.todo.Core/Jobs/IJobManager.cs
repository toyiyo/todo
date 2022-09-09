using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace toyiyo.todo.Jobs
{
    public interface IJobManager
    {
        Task<Job> Create(Job inputJob);
        Task<Job> Get(Guid id);
        Task<List<Job>> GetAll(GetAllJobsInput input);

        Task<int> GetAllCount(GetAllJobsInput input);
        Task<Job> Update(Job inputJob);
        Task Delete(Job job);
    }
}