using Abp.Application.Services.Dto;

namespace toyiyo.todo.Roles.Dto
{
    public class PagedRoleResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
    }
}

