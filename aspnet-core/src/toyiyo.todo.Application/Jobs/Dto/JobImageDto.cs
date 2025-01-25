
using System;
using Abp.Application.Services.Dto;

namespace toyiyo.todo.Jobs
{
    public class JobImageDto : EntityDto<Guid>
    {
        public Guid JobId { get; set; }
        public string ContentType { get; set; }
        public string FileName { get; set; }
        public byte[] ImageData { get; set; }
    }
}