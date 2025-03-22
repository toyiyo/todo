using System;

namespace toyiyo.todo.Projects
{
    public class ProjectHealthStatus
    {
        private const decimal RiskThreshold = 35m;
        private const decimal OnTrackThreshold = 65m;
        private const decimal BugRatioThreshold = 0.3m;

        public string Status { get; }
        public string CssClass { get; }

        private ProjectHealthStatus(string status, string cssClass)
        {
            Status = status;
            CssClass = cssClass;
        }

        public static ProjectHealthStatus Calculate(
            int totalJobCount,
            int completedTasks,
            int bugCount,
            DateTime? dueDate)
        {
            if (totalJobCount == 0)
                return new ProjectHealthStatus("Not Started", "badge-secondary");

            var completionRatio = (decimal)completedTasks / totalJobCount;
            var bugRatio = totalJobCount > 0 ? (decimal)bugCount / totalJobCount : 0;
            var isPastDue = dueDate.HasValue && DateTime.UtcNow > dueDate.Value;

            if (completionRatio == 1m)
                return new ProjectHealthStatus("Completed", "badge-success");

            if (isPastDue && completionRatio < 1m)
                return new ProjectHealthStatus("Overdue", "badge-danger");

            if (completionRatio >= (OnTrackThreshold / 100) && bugRatio <= BugRatioThreshold)
                return new ProjectHealthStatus("On Track", "badge-info");

            if (completionRatio >= (RiskThreshold / 100))
                return new ProjectHealthStatus("At Risk", "badge-warning");

            return new ProjectHealthStatus("Behind", "badge-danger");
        }
    }
}
