using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using toyiyo.todo.Notes;
using toyiyo.todo.Notes.Dto;
using toyiyo.todo.Jobs;
using toyiyo.todo.Jobs.Dto;
using toyiyo.todo.Projects;

namespace toyiyo.todo.Tests.Notes
{
    public class NoteAppService_Tests : todoTestBase
    {
        private readonly INoteAppService _noteAppService;
        private readonly IJobAppService _jobAppService;
        private readonly IProjectAppService _projectAppService;

        public NoteAppService_Tests()
        {
            _noteAppService = Resolve<INoteAppService>();
            _jobAppService = Resolve<IJobAppService>();
            _projectAppService = Resolve<IProjectAppService>();

            // Login as tenant admin
            LoginAsDefaultTenantAdmin();
        }

        [Fact]
        public async Task Should_Create_Note()
        {
            // Arrange
            var job = await CreateAndGetTestJob();
            
            var input = new CreateNoteInput
            {
                JobId = job.Id,
                Content = "Test note content"
            };

            // Act
            var result = await _noteAppService.CreateAsync(input);

            // Assert
            result.ShouldNotBeNull();
            result.Content.ShouldBe(input.Content);
            result.JobId.ShouldBe(input.JobId);
        }

        [Fact]
        public async Task Should_Create_Reply()
        {
            // Arrange
            var job = await CreateAndGetTestJob();
            
            var parentNote = await _noteAppService.CreateAsync(new CreateNoteInput
            {
                JobId = job.Id,
                Content = "Parent note"
            });

            var replyInput = new CreateNoteInput
            {
                JobId = job.Id,
                Content = "Reply content",
                ParentNoteId = parentNote.Id
            };

            // Act
            var result = await _noteAppService.CreateAsync(replyInput);

            // Assert
            result.ShouldNotBeNull();
            result.ParentNoteId.ShouldBe(parentNote.Id);
        }

        [Fact]
        public async Task Should_Get_All_Notes()
        {
            // Arrange
            var job = await CreateAndGetTestJob();
            await CreateTestNotes(job.Id, 3);

            var input = new GetNotesInput
            {
                JobId = job.Id,
                MaxResultCount = 10,
                SkipCount = 0
            };

            // Act
            var result = await _noteAppService.GetAllAsync(input);

            // Assert
            result.TotalCount.ShouldBe(3);
            result.Items.Count.ShouldBe(3);
        }

        [Fact]
        public async Task Should_Update_Note()
        {
            // Arrange
            var job = await CreateAndGetTestJob();
            var note = await _noteAppService.CreateAsync(new CreateNoteInput
            {
                JobId = job.Id,
                Content = "Initial content"
            });

            // Act
            var updatedNote = await _noteAppService.UpdateAsync(note.Id, "Updated content");

            // Assert
            updatedNote.Content.ShouldBe("Updated content");
        }

        private async Task<JobDto> CreateAndGetTestJob()
        {
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            
            var project = await _projectAppService.Create(new CreateProjectInputDto() { Title = "test project" });
            return await _jobAppService.Create(new JobCreateInputDto 
            { 
                ProjectId = project.Id, 
                Title = "test job", 
                Description = "test job" 
            });
        }

        private async Task CreateTestNotes(Guid jobId, int count)
        {
            for (int i = 0; i < count; i++)
            {
                await _noteAppService.CreateAsync(new CreateNoteInput
                {
                    JobId = jobId,
                    Content = $"Test note {i}"
                });
            }
        }
    }
}
