using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using static toyiyo.todo.Jobs.Job;

namespace toyiyo.todo.Jobs
{
    public class GetAllJobsInput : PagedAndSortedResultRequestDto
    {
        public string Keyword { get; set; }
        public Guid ProjectId { get; set; }
        public Status? JobStatus { get; set; }
        public Guid ParentJobId { get; set; }
        public JobLevel? Level { get; set; }
    }

}