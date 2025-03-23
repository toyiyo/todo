using System;
using System.Collections.Generic;
using Abp.Domain.Values;

namespace toyiyo.todo.Forecasting
{
    public class ForecastResult : ValueObject
    {
        public DateTime EstimatedCompletionDate { get; private set; }
        public DateTime OptimisticCompletionDate { get; private set; }
        public DateTime ConservativeCompletionDate { get; private set; }
        public decimal ConfidenceLevel { get; private set; }
        public List<ProgressPoint> ActualProgress { get; private set; }
        public List<ProgressPoint> ForecastProgress { get; private set; }
        public List<ProgressPoint> OptimisticProgress { get; private set; }
        public List<ProgressPoint> ConservativeProgress { get; private set; }

        protected ForecastResult() { }

        public static ForecastResult Create(
            DateTime estimatedDate,
            DateTime optimisticDate,
            DateTime conservativeDate,
            decimal confidence,
            List<ProgressPoint> actual,
            List<ProgressPoint> forecast,
            List<ProgressPoint> optimistic,
            List<ProgressPoint> conservative)
        {
            if (optimisticDate > estimatedDate)
                throw new ArgumentException("Optimistic date must be before or equal to estimated date");
            if (conservativeDate < estimatedDate)
                throw new ArgumentException("Conservative date must be after or equal to estimated date");
            if (confidence < 0 || confidence > 1)
                throw new ArgumentException("Confidence must be between 0 and 1");

            return new ForecastResult
            {
                EstimatedCompletionDate = estimatedDate,
                OptimisticCompletionDate = optimisticDate,
                ConservativeCompletionDate = conservativeDate,
                ConfidenceLevel = confidence,
                ActualProgress = actual,
                ForecastProgress = forecast,
                OptimisticProgress = optimistic,
                ConservativeProgress = conservative
            };
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return EstimatedCompletionDate;
            yield return OptimisticCompletionDate;
            yield return ConservativeCompletionDate;
            yield return ConfidenceLevel;
            yield return ActualProgress;
            yield return ForecastProgress;
            yield return OptimisticProgress;
            yield return ConservativeProgress;
        }
    }
}
