using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;

namespace toyiyo.todo.Projects.Dto
{
    [AutoMap(typeof(Project))]
    public class ProjectDto : EntityDto<Guid>
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int? TenantId { get; set; }
        public string TenantName { get; set; }
        public string CreatorUserName { get; set; }
        public string CreatorUserId { get; set; }
        public DateTime CreationTime { get; set; }
        public string LastModifierUserName { get; set; }
        public string LastModifierUserId { get; set; }
        public DateTime? LastModificationTime { get; set; }
        public string DeleterUserName { get; set; }
        public string DeleterUserId { get; set; }
        public DateTime? DeletionTime { get; set; }
    }
}
