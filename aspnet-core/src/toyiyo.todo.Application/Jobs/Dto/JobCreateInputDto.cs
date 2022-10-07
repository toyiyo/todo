using System;
using System.ComponentModel.DataAnnotations;

namespace toyiyo.todo.Jobs
{
    public class JobCreateInputDto
    {
        /// <summary>
        /// The job's title, there is a max length of 500 characters
        /// </summary>
        [Required]
        [StringLength(Job.MaxTitleLength)]
        public string Title { get; set; }
        /// <summary>
        /// The job's description, 2000000 characters max or roughly 2MB limit | 307692 - 400000 words | 1230.8 - 1600.0 pages
        /// </summary>
        [StringLength(Job.MaxDescriptionLength)]
        public string Description { get; set; }
        /// <summary>
        /// Identifier for the project that this job belongs to
        /// </summary>
        public Guid ProjectId { get; set; }
        /// <summary>
        /// Date when the job is due.  Due dates can't be set to the past
        /// </summary>
        public DateTime? DueDate {get; set;}
    }
}