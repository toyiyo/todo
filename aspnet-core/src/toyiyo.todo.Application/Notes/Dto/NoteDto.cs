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

    public class CreateNoteInput
    {
        public string Content { get; set; }
        public Guid JobId { get; set; }
        public Guid? ParentNoteId { get; set; }
    }

    public class GetNotesInput
    {
        public Guid JobId { get; set; }
        public string Keyword { get; set; }
        public int MaxResultCount { get; set; }
        public int SkipCount { get; set; }
    }

    public class UpdateNoteInput
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
    }
}
