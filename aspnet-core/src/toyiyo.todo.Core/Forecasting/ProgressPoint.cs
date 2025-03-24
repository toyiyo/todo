using System;
using System.Collections.Generic;
using Abp.Domain.Values;

namespace toyiyo.todo.Forecasting
{
    public class ProgressPoint : ValueObject
    {
        public DateTime Date { get; private set; }
        public int CompletedTasks { get; private set; }
        public int TotalTasks { get; private set; }

        protected ProgressPoint() { }

        public static ProgressPoint Create(DateTime date, int completed, int total)
        {
            if (completed > total)
                throw new ArgumentException("Completed tasks cannot exceed total tasks");

            return new ProgressPoint
            {
                Date = date,
                CompletedTasks = completed,
                TotalTasks = total
            };
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Date;
            yield return CompletedTasks;
            yield return TotalTasks;
        }
    }
}
