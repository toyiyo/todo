using System;
using System.Collections.Generic;

namespace toyiyo.todo.Jobs.Dto
{
    /// <summary>
    /// Data transfer object for roadmap view
    /// </summary>
    public class RoadmapViewDto
    {
        /// <summary>
        /// Start date of the roadmap view
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date of the roadmap view
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// List of jobs in the roadmap
        /// </summary>
        public List<JobDto> Jobs { get; set; }
    }
}