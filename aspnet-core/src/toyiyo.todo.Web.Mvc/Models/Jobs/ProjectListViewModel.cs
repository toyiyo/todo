using System.Collections.Generic;
using toyiyo.todo.Jobs.Dto;

namespace toyiyo.todo.Web.Models.Jobs
{
    public class JobListViewModel
    {
        public JobListViewModel()
        {
        }

        public IReadOnlyList<JobDto> Jobs { get; set; }
    }
}