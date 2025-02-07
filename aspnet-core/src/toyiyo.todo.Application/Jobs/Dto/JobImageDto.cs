
using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;

namespace toyiyo.todo.Jobs
{
    [AutoMap(typeof(JobImage))]
    public class JobImageDto : EntityDto<Guid>
    {
        public Guid JobId { get; set; }
        public string ContentType { get; set; }
        public string FileName { get; set; }
        public string imageUrl { get; set; }
    }
}