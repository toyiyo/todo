
using System;
using System.ComponentModel.DataAnnotations;

namespace toyiyo.todo.Jobs
{
    public class JobImageCreateInputDto
    {
        [Required]
        public Guid JobId { get; set; }
        
        [Required]
        [StringLength(255)]
        public string ContentType { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; }

        [Required]
        public byte[] ImageData { get; set; }
    }
}