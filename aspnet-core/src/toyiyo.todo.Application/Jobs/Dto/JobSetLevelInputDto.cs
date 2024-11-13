
using System;
using System.ComponentModel.DataAnnotations;
using static toyiyo.todo.Jobs.Job;

namespace toyiyo.todo.Jobs.Dto
{
    public class JobSetLevelInputDto
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        [EnumDataType(typeof(JobLevel))]
        public JobLevel Level { get; set; }
    }
}