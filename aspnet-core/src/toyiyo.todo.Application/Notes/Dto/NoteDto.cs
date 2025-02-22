using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;
using AutoMapper;
using toyiyo.todo.Notes;

namespace toyiyo.todo.Notes.Dto
{
    /// <summary>
    /// Data Transfer Object (DTO) for Note entity.
    /// </summary>
    [AutoMap(typeof(Note))]
    public class NoteDto : EntityDto<Guid>
    {
        /// <summary>
        /// Gets or sets the content of the note.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the Job ID associated with the note.
        /// </summary>
        public Guid JobId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the parent note, if any.
        /// </summary>
        public Guid? ParentNoteId { get; set; }

        /// <summary>
        /// Gets or sets the creation time of the note.
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// Gets or sets the name of the author of the note.
        /// </summary>
        public string AuthorName { get; set; }

        /// <summary>
        /// Gets or sets the email of the author of the note.
        /// </summary>
        public string AuthorEmail { get; set; }

        /// <summary>
        /// Gets or sets the ID of the author of the note.
        /// </summary>
        public long? AuthorId { get; set; }

        /// <summary>
        /// Gets or sets the list of replies to the note.
        /// </summary>
        public List<NoteDto> Replies { get; set; } = new List<NoteDto>();

        /// <summary>
        /// Creates the mapping configuration for Note to NoteDto.
        /// </summary>
        /// <param name="configuration">The AutoMapper configuration expression.</param>
        public static void CreateMapping(IMapperConfigurationExpression configuration)
        {
            configuration.CreateMap<Note, NoteDto>()
                .ForMember(dto => dto.AuthorId, opt => opt.MapFrom(n => n.CreatorUserId))
                .ForMember(dto => dto.AuthorEmail, opt => opt.MapFrom(n => n.Author != null ? n.Author.EmailAddress : null))
                .ForMember(dto => dto.AuthorName, opt => opt.MapFrom(n => n.Author != null ? n.Author.UserName : null));
        }
    }

/// <summary>
/// Input DTO for creating a new note.
/// </summary>
    public class CreateNoteInput
    {
        /// <summary>
        /// Gets or sets the content of the note.
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// Gets or sets the Job ID associated with the note.
        /// </summary>
        public Guid JobId { get; set; }
        /// <summary>
        /// Gets or sets the ID of the parent note, if any.
        /// </summary>
        public Guid? ParentNoteId { get; set; }
    }

    /// <summary>
    /// Input DTO for getting all notes for a specific job.
    /// </summary>
    public class GetNotesInput
    {
        /// <summary>
        /// Gets or sets the Job ID associated with the note.
        /// </summary>
        public Guid JobId { get; set; }
        /// <summary>
        /// Gets or sets the keyword to filter notes by content.
        /// </summary>
        public string Keyword { get; set; }
        /// <summary>
        ///     Gets or sets the maximum number of results to return.
        /// </summary>
        public int MaxResultCount { get; set; }
        /// <summary>
        ///    Gets or sets the number of results to skip.
        /// </summary>
        public int SkipCount { get; set; }
    }

    /// <summary>
    /// Input DTO for updating an existing note.
    /// </summary>
    public class UpdateNoteInput
    {
        /// <summary>
        /// Gets or sets the ID of the note to update.
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Gets or sets the new content of the note.
        /// </summary>
        public string Content { get; set; }
    }
}
