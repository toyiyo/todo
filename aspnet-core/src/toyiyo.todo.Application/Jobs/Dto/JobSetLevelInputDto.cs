using System;
using System.ComponentModel.DataAnnotations;
using static toyiyo.todo.Jobs.Job;

/// <summary>
/// Data Transfer Object for setting the level of a job.
/// </summary>
/// <remarks>
/// This DTO is used to update the level of an existing job by providing the job's ID and the new level.
/// </remarks>
namespace toyiyo.todo.Jobs.Dto
{
    /// <summary>
    /// Data Transfer Object for updating the level of a job.
    /// </summary>
    public class JobSetLevelInputDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the job to update.
        /// </summary>
        [Required]
        public Guid Id { get; set; }
        /// <summary>
        /// Gets or sets the new level to be assigned to the job.
        /// </summary>
        [Required]
        [EnumDataType(typeof(JobLevel))]
        public JobLevel Level { get; set; }
    }
}