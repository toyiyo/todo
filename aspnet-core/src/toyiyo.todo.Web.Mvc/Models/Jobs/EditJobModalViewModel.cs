using Abp.AutoMapper;
using toyiyo.todo.Jobs.Dto;
using toyiyo.todo.Web.Views.Shared.Components.UserDropdown;

namespace toyiyo.todo.Web.Models.Jobs
{
    [AutoMapFrom(typeof(JobDto))]
    public class EditJobModalViewModel : JobDto
    {
        public EditJobModalViewModel()
        {
        }
        //add calculated properties here
 
        public int JobStatusId { get => (int)JobStatus; }
         public UserDropdownViewModel UserDropdown { get; set; } // Include UserDropdownViewModel
    }
}