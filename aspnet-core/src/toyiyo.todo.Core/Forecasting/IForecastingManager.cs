using System;
using System.Threading.Tasks;
using toyiyo.todo.Projects;
using toyiyo.todo.Jobs;
using System.Collections.Generic;

namespace toyiyo.todo.Forecasting
{
    public interface IForecastingManager
    {
        Task<ForecastResult> CalculateForecast(Project project, Job.JobLevel level);
        Task<List<ProgressPoint>> GetHistoricalProgress(Guid projectId, Job.JobLevel level);
    }
}
