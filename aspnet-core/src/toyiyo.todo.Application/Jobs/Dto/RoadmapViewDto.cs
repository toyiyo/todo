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
        /// Type of view for the roadmap
        /// </summary>
        public ViewType ViewTypeValue { get; set; }

        /// <summary>
        /// List of jobs in the roadmap
        /// </summary>
        public List<JobDto> Jobs { get; set; }

        /// <summary>
        /// Defines the type of view for the roadmap
        /// </summary>
        public enum ViewType
        {
            Monthly,
            Quarterly
        }
    }
}