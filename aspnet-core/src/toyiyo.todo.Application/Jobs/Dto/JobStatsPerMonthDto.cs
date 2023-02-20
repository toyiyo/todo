using System;

namespace toyiyo.todo.Jobs.Dto
{
    public class JobStatsPerMonthDto
    {
        public int Year { get; internal set; }
        public string Month { get; internal set; }
        public int Count { get; internal set; }
    }
}