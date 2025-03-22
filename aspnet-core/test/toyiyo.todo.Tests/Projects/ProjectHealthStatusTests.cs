using System;
using Shouldly;
using Xunit;
using toyiyo.todo.Projects;


namespace toyiyo.todo.Tests.Projects
{
    public class ProjectHealthStatusTests : todoTestBase
    {
        [Fact]
        public void Calculate_WhenNoTasks_ShouldReturnNotStarted()
        {
            // Arrange
            int TotalJobCount = 0;
            int completedTasks = 0;
            int bugCount = 0;
            DateTime? dueDate = null;

            // Act
            var result = ProjectHealthStatus.Calculate(TotalJobCount, completedTasks, bugCount, dueDate);

            // Assert
            result.Status.ShouldBe("Not Started");
            result.CssClass.ShouldBe("badge-secondary");
        }

        [Fact]
        public void Calculate_WhenAllTasksComplete_ShouldReturnCompleted()
        {
            // Arrange
            int TotalJobCount = 100;
            int completedTasks = 100; // 100% completion ratio
            int bugCount = 10;
            DateTime? dueDate = DateTime.UtcNow.AddDays(10);

            // Act
            var result = ProjectHealthStatus.Calculate(TotalJobCount, completedTasks, bugCount, dueDate);

            // Assert
            result.Status.ShouldBe("Completed");
            result.CssClass.ShouldBe("badge-success");
        }

        [Fact]
        public void Calculate_WhenPastDueAndNotComplete_ShouldReturnOverdue()
        {
            // Arrange
            int TotalJobCount = 100;
            int completedTasks = 90; // 90% completion ratio
            int bugCount = 5;
            DateTime? dueDate = DateTime.UtcNow.AddDays(-1); // Past due

            // Act
            var result = ProjectHealthStatus.Calculate(TotalJobCount, completedTasks, bugCount, dueDate);

            // Assert
            result.Status.ShouldBe("Overdue");
            result.CssClass.ShouldBe("badge-danger");
        }

        [Theory]
        [InlineData(65, 10, 0.1)] // Just at threshold
        [InlineData(80, 20, 0.25)] // Well above threshold
        [InlineData(99, 30, 0.3)] // At bug ratio limit
        public void Calculate_WhenHighCompletionAndLowBugRatio_ShouldReturnOnTrack(
            int completionPercentage, int bugCount, decimal bugRatio)
        {
            // Arrange
            int TotalJobCount = 100;
            int completedTasks = completionPercentage; // completionRatio = completionPercentage/100
            DateTime? dueDate = DateTime.UtcNow.AddDays(10);

            // Act
            var result = ProjectHealthStatus.Calculate(TotalJobCount, completedTasks, bugCount, dueDate);

            // Assert
            result.Status.ShouldBe("On Track");
            result.CssClass.ShouldBe("badge-info");
        }

        [Theory]
        [InlineData(35, 0)] // Just at risk threshold
        [InlineData(50, 31)] // Above risk threshold but high bug count
        public void Calculate_WhenMediumCompletionOrHighBugRatio_ShouldReturnAtRisk(
            int completionPercentage, int bugCount)
        {
            // Arrange
            int TotalJobCount = 100;
            int completedTasks = completionPercentage; // completionRatio = completionPercentage/100
            DateTime? dueDate = DateTime.UtcNow.AddDays(10);

            // Act
            var result = ProjectHealthStatus.Calculate(TotalJobCount, completedTasks, bugCount, dueDate);

            // Assert
            result.Status.ShouldBe("At Risk");
            result.CssClass.ShouldBe("badge-warning");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(10)]
        [InlineData(34)] // Just below risk threshold
        public void Calculate_WhenLowCompletion_ShouldReturnBehind(int completionPercentage)
        {
            // Arrange
            int TotalJobCount = 100;
            int completedTasks = completionPercentage; // completionRatio = completionPercentage/100
            int bugCount = 10;
            DateTime? dueDate = DateTime.UtcNow.AddDays(10);

            // Act
            var result = ProjectHealthStatus.Calculate(TotalJobCount, completedTasks, bugCount, dueDate);

            // Assert
            result.Status.ShouldBe("Behind");
            result.CssClass.ShouldBe("badge-danger");
        }

        [Fact]
        public void Calculate_WithEdgeCaseCompletionRatio_ShouldHandleCorrectly()
        {
            // Arrange - exactly at threshold boundaries
            int TotalJobCount = 100;
            int justBelowRiskThreshold = 34;
            int exactlyAtRiskThreshold = 35;
            int justBelowOnTrackThreshold = 64;
            int exactlyAtOnTrackThreshold = 65;
            int bugCount = 10; // Keep bug ratio below threshold
            DateTime? dueDate = DateTime.UtcNow.AddDays(10);

            // Act & Assert - Testing the threshold boundaries
            ProjectHealthStatus.Calculate(TotalJobCount, justBelowRiskThreshold, bugCount, dueDate)
                .Status.ShouldBe("Behind");

            ProjectHealthStatus.Calculate(TotalJobCount, exactlyAtRiskThreshold, bugCount, dueDate)
                .Status.ShouldBe("At Risk");

            ProjectHealthStatus.Calculate(TotalJobCount, justBelowOnTrackThreshold, bugCount, dueDate)
                .Status.ShouldBe("At Risk");

            ProjectHealthStatus.Calculate(TotalJobCount, exactlyAtOnTrackThreshold, bugCount, dueDate)
                .Status.ShouldBe("On Track");
        }

        [Fact]
        public void Calculate_WithDifferentCompletionRatios_ShouldReflectCorrectProgress()
        {
            // Arrange
            int TotalJobCount = 200;
            int[] completedTasksArray = { 0, 70, 130, 200 };
            int bugCount = 10;
            DateTime? dueDate = DateTime.UtcNow.AddDays(10);

            string[] expectedStatuses = { "Behind", "At Risk", "On Track", "Completed" };

            // Act & Assert
            for (int i = 0; i < completedTasksArray.Length; i++)
            {
                int completedTasks = completedTasksArray[i];
                string expectedStatus = expectedStatuses[i];

                var result = ProjectHealthStatus.Calculate(TotalJobCount, completedTasks, bugCount, dueDate);
                result.Status.ShouldBe(expectedStatus,
                    $"Failed with completionRatio = {(decimal)completedTasks / TotalJobCount:P0}");
            }
        }
    }
}