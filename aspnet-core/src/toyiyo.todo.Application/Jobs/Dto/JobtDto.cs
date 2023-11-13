using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using toyiyo.todo.Projects.Dto;
using toyiyo.todo.Users.Dto;
using static toyiyo.todo.Jobs.Job;

namespace toyiyo.todo.Jobs.Dto
{
    [AutoMap(typeof(Job))]
    public class JobDto : EntityDto<Guid>
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        //todo: updating mappings to return ids and names rather than full objects
        public ProjectDto Project { get; set; }
        public UserDto Owner { get; set; }
        public UserDto Assignee { get; set; }
        public Status JobStatus { get; set; }
        public bool IsDeleted { get; set; }
        public string DeleterUserId { get; set; }
        public DateTime? DeletionTime { get; set; }
        public string CreatorUserId { get; set; }
        public DateTime CreationTime { get; set; }
        public string LastModifierUserId { get; set; }
        public DateTime? LastModificationTime { get; set; }
        public Guid? ParentId { get; set; }
        public int? TenantId { get; set; }
        public DateTime OrderByDate { get; set; }
        public JobLevel Level { get; set; }
    }
}
