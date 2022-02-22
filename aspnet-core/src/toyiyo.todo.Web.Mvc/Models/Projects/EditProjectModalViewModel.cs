using Abp.AutoMapper;
using toyiyo.todo.Projects.Dto;

namespace toyiyo.todo.Web.Models.Projects
{
    [AutoMapFrom(typeof(ProjectDto))]
    public class EditProjectModalViewModel : ProjectDto
    {
        public EditProjectModalViewModel()
        {
        }
        //add calculated properties here
    }
}