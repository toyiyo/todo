using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using toyiyo.todo.Jobs.Validators;

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
        [FileExtensions(Extensions = "jpg,png,jpeg,gif", ErrorMessage = "Please upload a valid image file (jpg, png, jpeg, gif).")]
        public string FileName { get; set; }

        [Required]
        [DataType(DataType.Upload)]
        [MaxFileSize(5 * 1024 * 1024)]
        public IFormFile ImageData { get; set; }
    }
}