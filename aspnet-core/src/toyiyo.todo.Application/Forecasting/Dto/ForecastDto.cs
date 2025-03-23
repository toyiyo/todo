using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;

namespace toyiyo.todo.Forecasting.Dto
{
    [AutoMapFrom(typeof(ForecastResult))]
    public class ForecastResultDto : EntityDto
    {
        public DateTime EstimatedCompletionDate { get; set; }
        public DateTime OptimisticCompletionDate { get; set; }
        public DateTime ConservativeCompletionDate { get; set; }
        public decimal ConfidenceLevel { get; set; }
        public List<ProgressPointDto> ActualProgress { get; set; }
        public List<ProgressPointDto> ForecastProgress { get; set; }
        public List<ProgressPointDto> OptimisticProgress { get; set; }
        public List<ProgressPointDto> ConservativeProgress { get; set; }
    }

    [AutoMapFrom(typeof(ProgressPoint))]
    public class ProgressPointDto
    {
        public DateTime Date { get; set; }
        public int CompletedTasks { get; set; }
        public int TotalTasks { get; set; }
        public decimal CompletionPercentage => TotalTasks > 0 ? (decimal)CompletedTasks / TotalTasks * 100 : 0;
    }
}
