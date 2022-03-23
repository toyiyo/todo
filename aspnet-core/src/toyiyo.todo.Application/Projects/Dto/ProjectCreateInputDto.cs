using System.ComponentModel.DataAnnotations;

namespace toyiyo.todo.Projects
{
    public class CreateProjectInputDto
    {
        [Required]
        [StringLength(Project.MaxTitleLength)]
        public string Title { get; set; }
        public string Description { get; set; }
    }
}