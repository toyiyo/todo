using System.Collections.Generic;

namespace toyiyo.todo.Jobs.Dto
{
    /// <summary>
    /// Job statistics
    /// </summary>
    public class JobStatsDto
    {
        public int TotalCompletedJobs { get; internal set; }
        public int TotalJobs { get; internal set; }
        public int TotalOpenJobs { get; internal set; }
        public int TotalInProgressJobs { get; internal set; }
        public List<JobStatsPerMonthDto> TotalCompletedJobsPerMonth { get; set; }
    }
}