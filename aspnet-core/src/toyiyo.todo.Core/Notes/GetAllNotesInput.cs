using System;

namespace toyiyo.todo.Notes
{
    public class GetAllNotesInput
    {
        public Guid JobId { get; set; }
        public string Keyword { get; set; }
        public int MaxResultCount { get; set; }
        public int SkipCount { get; set; }
    }
}
