using Abp.AutoMapper;
using toyiyo.todo.Projects;

namespace toyiyo.todo.Web.Models.Projects
{
    [AutoMapFrom(typeof(CreateProjectInputDto))]
    public class EditProjectModalViewModel : CreateProjectInputDto
    {
        public EditProjectModalViewModel()
        {
        }
        //add calculated properties here
    }
}