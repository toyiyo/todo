
using static toyiyo.todo.Jobs.Job;

namespace toyiyo.todo.Jobs.Dto
{
    public class JobUpdateInputDto
    {
        public System.Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public System.DateTime? DueDate { get; set; }
        public JobLevel Level { get; set; }
    }
}