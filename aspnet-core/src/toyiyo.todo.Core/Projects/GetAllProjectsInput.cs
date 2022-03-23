using Abp.Application.Services.Dto;

namespace toyiyo.todo.Projects
{
    public class GetAllProjectsInput : PagedAndSortedResultRequestDto
    {
        public string keyword { get; set; }
    }

}