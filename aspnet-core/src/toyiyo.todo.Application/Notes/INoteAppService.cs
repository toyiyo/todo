using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using toyiyo.todo.Notes.Dto;

namespace toyiyo.todo.Notes
{
    public interface INoteAppService : IApplicationService
    {
        Task<NoteDto> CreateAsync(CreateNoteInput input);
        Task<PagedResultDto<NoteDto>> GetAllAsync(GetNotesInput input);
        Task DeleteAsync(Guid id);
        Task<NoteDto> UpdateAsync(Guid id, string content);
    }
}
