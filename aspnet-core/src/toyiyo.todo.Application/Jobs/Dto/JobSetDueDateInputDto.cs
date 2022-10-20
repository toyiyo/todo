using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;

namespace toyiyo.todo.Jobs
{
    /// <summary>
    /// Input DTO for setting a job's due date
    /// </summary>
    public class JobSetDueDateInputDto : EntityDto<Guid>
    {
        //accept a nullable date, default to null
        /// <summary>
        /// The date the job is due, or null if no due date
        /// </summary>
        public DateTime? DueDate { get; set; }
    }
}