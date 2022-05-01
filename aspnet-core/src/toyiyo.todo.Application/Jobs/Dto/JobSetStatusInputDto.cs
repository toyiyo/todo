using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using static toyiyo.todo.Jobs.Job;

namespace toyiyo.todo.Jobs
{
    public class JobSetStatusInputDto : EntityDto<Guid>
    {
        [Required]
        public Status JobStatus { get; set; }
    }
}