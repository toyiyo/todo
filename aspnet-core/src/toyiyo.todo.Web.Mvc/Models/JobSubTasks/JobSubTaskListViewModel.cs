using System.Collections.Generic;
using toyiyo.todo.Jobs.Dto;

namespace toyiyo.todo.Web.Models.Jobs
{
    public class JobSubTaskListViewModel
    {
        public JobSubTaskListViewModel()
        {
        }

        public IReadOnlyList<JobDto> Jobs { get; set; }
    }
}