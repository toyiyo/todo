using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;

namespace toyiyo.todo.Jobs
{
    public class JobSetParentInputDto : EntityDto<Guid>
    {
        [Required]
        public Guid? ParentId { get; set; }
    }
}