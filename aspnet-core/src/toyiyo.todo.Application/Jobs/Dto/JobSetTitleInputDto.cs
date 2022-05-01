using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;

namespace toyiyo.todo.Jobs
{
    public class JobSetTitleInputDto : EntityDto<Guid>
    {
        [Required]
        [StringLength(Job.MaxTitleLength)]
        public string Title { get; set; }
    }
}