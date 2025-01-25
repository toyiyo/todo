using System;
using System.Threading.Tasks;
using toyiyo.todo.Authorization.Users;

namespace toyiyo.todo.Jobs
{
    public interface IJobImageManager
    {
        Task<JobImage> Get(Guid id);
        Task<JobImage> Create(JobImage jobImage);
        Task Delete(Guid id, User user);
    }
}