using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;

namespace toyiyo.todo.Jobs
{
    public class JobSetDescriptionInputDto : EntityDto<Guid>
    {
        [Required]
        [StringLength(Job.MaxDescriptionLength)]
        public string Description { get; set; }
    }
}