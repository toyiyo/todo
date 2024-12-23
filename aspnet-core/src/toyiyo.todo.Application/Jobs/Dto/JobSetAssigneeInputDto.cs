using System;
using Abp.Application.Services.Dto;

namespace toyiyo.todo.Jobs
{
    public class JobSetAssigneeInputDto: EntityDto<Guid>
    {
        /// <summary>
        /// Gets or sets the identifier of the assignee.
        /// </summary>
        /// <value>
        /// The unique identifier of the assignee. This value is nullable.
        /// </value>
        public long? AssigneeId { get; set; }
        
    }
}