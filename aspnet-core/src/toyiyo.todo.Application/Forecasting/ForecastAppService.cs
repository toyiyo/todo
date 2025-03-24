using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using toyiyo.todo.Forecasting.Dto;
using toyiyo.todo.Projects;
using toyiyo.todo.Jobs;

namespace toyiyo.todo.Forecasting
{
    public class ForecastAppService : todoAppServiceBase, IForecastAppService
    {
        private readonly IForecastingManager _forecastingManager;
        private readonly IProjectManager _projectManager;

        public ForecastAppService(
            IForecastingManager forecastingManager,
            IProjectManager projectManager)
        {
            _forecastingManager = forecastingManager;
            _projectManager = projectManager;
        }

        public async Task<ForecastResultDto> GetForecast(Guid projectId, Job.JobLevel level)
        {
            var project = await _projectManager.Get(projectId);
            var forecast = await _forecastingManager.CalculateForecast(project, level);
            
            return ObjectMapper.Map<ForecastResultDto>(forecast);
        }
    }
}
