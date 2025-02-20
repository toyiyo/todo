using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
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
        public List<NoteDto> Replies { get; set; } = new List<NoteDto>();
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
}
