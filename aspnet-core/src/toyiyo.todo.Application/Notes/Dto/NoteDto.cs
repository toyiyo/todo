using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;
using AutoMapper;
using toyiyo.todo.Notes;

namespace toyiyo.todo.Notes.Dto
{
    [AutoMap(typeof(Note))]
    public class NoteDto : EntityDto<Guid>
    {
        public string Content { get; set; }
        public Guid JobId { get; set; }
        public Guid? ParentNoteId { get; set; }
        public DateTime CreationTime { get; set; }
        public string AuthorName { get; set; }
        public string AuthorEmail { get; set; }
        public long? AuthorId { get; set; }
        public List<NoteDto> Replies { get; set; } = new List<NoteDto>();

        public void CreateMapping(IMapperConfigurationExpression configuration)
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
