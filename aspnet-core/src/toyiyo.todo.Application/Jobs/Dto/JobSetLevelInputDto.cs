
using System;
using static toyiyo.todo.Jobs.Job;

namespace toyiyo.todo.Jobs.Dto
{
    public class JobSetLevelInputDto
    {
        public Guid Id { get; set; }
        public JobLevel Level { get; set; }
    }
}