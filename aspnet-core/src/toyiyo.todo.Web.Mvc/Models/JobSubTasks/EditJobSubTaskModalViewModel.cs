using Abp.AutoMapper;
using toyiyo.todo.Jobs.Dto;

namespace toyiyo.todo.Web.Models.Jobs
{
    [AutoMapFrom(typeof(JobDto))]
    public class EditJobSubTaskModalViewModel : JobDto
    {
        public EditJobSubTaskModalViewModel()
        {
        }
        //add calculated properties here

        public int JobStatusId { get => (int)this.JobStatus; }
    }
}