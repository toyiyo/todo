using System.Collections.Generic;
using toyiyo.todo.Projects.Dto;

namespace toyiyo.todo.Web.Models.Projects
{
    public class ProjectListViewModel
    {
        public ProjectListViewModel()
        {
        }

        public IReadOnlyList<ProjectDto> Projects { get; set; }
    }
}