using System;
using Abp.Application.Services.Dto;

namespace toyiyo.todo.Jobs
{
    /// <summary>
    /// DTO to set the order by date for a job, this is used when manually ordering a collection of jobs
    /// </summary>
    public class JobPatchOrderByDateInputDto : EntityDto<Guid>
    {
        /// <summary>
        /// The order by date for a job
        /// </summary>
        public DateTime OrderByDate {get; set;}
    }
}