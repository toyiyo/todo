using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using toyiyo.todo.Forecasting.Dto;
using toyiyo.todo.Jobs;

namespace toyiyo.todo.Forecasting
{
    public interface IForecastAppService : IApplicationService
    {
        Task<ForecastResultDto> GetForecast(Guid projectId, Job.JobLevel level);
    }
}
