
using System.ComponentModel.DataAnnotations;
using static toyiyo.todo.Jobs.Job;

namespace toyiyo.todo.Jobs.Dto
{
    /// <summary>
    /// Data transfer object for updating a job entity.
    /// </summary>
    /// <remarks>
    /// This DTO contains all the necessary fields to update an existing job in the system.
    /// </remarks>
    /// <seealso cref="Job"/>
    public class JobUpdateInputDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the job.
        /// </summary>
        [Required]
        public System.Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the title of the job.
        /// Must not exceed the maximum length defined in Job.MaxTitleLength.
        /// </summary>
        [Required]
        [StringLength(Job.MaxTitleLength)]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description of the job.
        /// Must not exceed the maximum length defined in Job.MaxDescriptionLength.
        /// </summary>
        [StringLength(Job.MaxDescriptionLength)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the due date of the job.
        /// Can be null if no due date is set.
        /// </summary>
        public System.DateTime? DueDate { get; set; }

        /// <summary>
        /// Gets or sets the importance level of the job.
        /// </summary>
        [Required]
        public JobLevel Level { get; set; }

        /// <summary>
        /// Gets or sets the parent job identifier.
        /// </summary>
        public System.Guid ParentId { get; set; }
    }
}