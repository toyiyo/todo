using System;
using System.ComponentModel.DataAnnotations;

namespace toyiyo.todo.Jobs
{
    public class JobCreateInputDto
    {
        [Required]
        [StringLength(Job.MaxTitleLength)]
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid ProjectId { get; set; }
    }
}