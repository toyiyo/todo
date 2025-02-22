using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using toyiyo.todo.Notes.Dto;

namespace toyiyo.todo.Notes
{
    /// <summary>
    /// Interface for the Note application service.
    /// </summary>
    public interface INoteAppService : IApplicationService
    {
        /// <summary>
        /// Creates a new note.
        /// </summary>
        /// <param name="input">The input containing note details.</param>
        /// <returns>The created note DTO.</returns>
        Task<NoteDto> CreateAsync(CreateNoteInput input);

        /// <summary>
        /// Gets all notes for a specific job with optional filtering and pagination.
        /// </summary>
        /// <param name="input">Input parameters containing JobId, Keyword and paging information.</param>
        /// <returns>Paged list of notes.</returns>
        Task<PagedResultDto<NoteDto>> GetAllAsync(GetNotesInput input);

        /// <summary>
        /// Deletes a note by its ID.
        /// </summary>
        /// <param name="id">The ID of the note to delete.</param>
        Task DeleteAsync(Guid id);

        /// <summary>
        /// Updates the content of a note.
        /// </summary>
        /// <param name="id">The ID of the note to update.</param>
        /// <param name="content">The new content of the note.</param>
        /// <returns>The updated note DTO.</returns>
        Task<NoteDto> UpdateAsync(Guid id, string content);
    }
}
