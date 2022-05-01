using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;

namespace toyiyo.todo.Jobs
{
    public class JobSetDueDateInputDto : EntityDto<Guid>
    {
        [Required]
        public DateTime DueDate { get; set; }
    }
}