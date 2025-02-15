using System;
using System.ComponentModel.DataAnnotations;
using static toyiyo.todo.Jobs.Job;

namespace toyiyo.todo.Jobs
{
    public class JobCreateInputDto
    {
        /// <summary>
        /// Validates that the start date is not after the due date.
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
        /// Date when the job is due.  Due dates can't be set to the past.  Date is UTC
        /// </summary>
        public DateTime? DueDate {get; set;}

        public Guid? ParentId { get; set;}
        public JobLevel Level { get; set; }
        /// <summary>
        /// Date when the job starts.  Start dates can't be set to the past.  Date is UTC
        /// </summary>
        [CustomValidation(typeof(JobCreateInputDto), nameof(ValidateStartDate))]
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// Validation method for start date
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        public static ValidationResult ValidateStartDate(DateTime? startDate, ValidationContext validationContext)
        {
            var instance = (JobCreateInputDto)validationContext.ObjectInstance;
            if (startDate.HasValue && instance.DueDate.HasValue && startDate.Value > instance.DueDate.Value)
            {
                return new ValidationResult("Start date cannot be after due date");
            }
            return ValidationResult.Success;
        }
    }
}