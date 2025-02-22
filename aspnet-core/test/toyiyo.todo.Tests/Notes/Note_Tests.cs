using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using toyiyo.todo.Notes;
using toyiyo.todo.Authorization.Users;
using toyiyo.todo.Jobs;
using toyiyo.todo.Projects;

namespace toyiyo.todo.Tests.Notes
{
    public class Note_Tests : todoTestBase
    {
        private readonly IJobAppService _jobAppService;
        private readonly IProjectAppService _projectAppService;
        private readonly IJobManager _jobManager;
        private readonly IProjectManager _projectManager;

        public Note_Tests()
        {
            _jobAppService = Resolve<IJobAppService>();
            _projectAppService = Resolve<IProjectAppService>();
            _jobManager = Resolve<IJobManager>();
            _projectManager = Resolve<IProjectManager>();

            // Login as tenant admin
            LoginAsDefaultTenantAdmin();
        }

        [Fact]
        public async Task Should_Create_Note()
        {
            // Arrange
            LoginAsDefaultTenantAdmin();
            var user = await GetCurrentUserAsync();
            var job = await CreateTestJob();
            var content = "Test note content";

            // Act
            var note = Note.Create(job, content, user);

            // Assert
            note.ShouldNotBeNull();
            note.Content.ShouldBe(content);
            note.JobId.ShouldBe(job.Id);
            note.TenantId.ShouldBe(AbpSession.TenantId.Value);
            note.CreatorUserId.ShouldBe(user.Id);
            note.ParentNoteId.ShouldBeNull();
        }

        private async Task<Job> CreateTestJob()
        {
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            
            var project = await _projectAppService.Create(new CreateProjectInputDto() { Title = "test project" });
            var jobDto = await _jobAppService.Create(new JobCreateInputDto 
            { 
                ProjectId = project.Id, 
                Title = "test job", 
                Description = "test job" 
            });

            return await _jobManager.Get(jobDto.Id);
        }

        [Fact]
        public async Task Should_Create_ReplyAsync()
        {
            // Arrange
            var job = await CreateTestJob();
            var author = await GetCurrentUserAsync();
            var parentNote = Note.Create(job, "Parent note", author);
            var replyContent = "Reply content";

            // Act
            var reply = Note.Create(job, replyContent, author, parentNote);

            // Assert
            reply.ShouldNotBeNull();
            reply.Content.ShouldBe(replyContent);
            reply.ParentNoteId.ShouldBe(parentNote.Id);
        }

        [Fact]
        public async Task Should_Update_ContentAsync()
        {
            // Arrange
            var job = await CreateTestJob();
            var author = await GetCurrentUserAsync();
            var note = Note.Create(job, "Initial content", author);
            var newContent = "Updated content";

            // Act
            var updatedNote = Note.SetContent(note, newContent, author);

            // Assert
            updatedNote.Content.ShouldBe(newContent);
            updatedNote.LastModifierUserId.ShouldBe(author.Id);
        }

        [Fact]
        public async Task Should_Not_Update_Content_By_Different_UserAsync()
        {
            // Arrange
            var job = await CreateTestJob();
            var author = await GetCurrentUserAsync();
            var differentUser = new User { Id = 5, UserName = "otheruser" };
            var note = Note.Create(job, "Initial content", author);

            // Act & Assert
            Should.Throw<UnauthorizedAccessException>(() =>
                Note.SetContent(note, "New content", differentUser)
            );
        }

        [Fact]
        public async Task Should_Delete_NoteAsync()
        {
            // Arrange
            var job = await CreateTestJob();
            var author = await GetCurrentUserAsync();
            var note = Note.Create(job, "Content", author);

            // Act
            var deletedNote = Note.Delete(note, author);

            // Assert
            deletedNote.IsDeleted.ShouldBeTrue();
            deletedNote.DeleterUserId.ShouldBe(author.Id);
        }

        [Fact]
        public async Task Should_Not_Delete_By_Different_UserAsync()
        {
            // Arrange
            var job = await CreateTestJob();
            var author = await GetCurrentUserAsync();
            var differentUser = new User { Id = 5, UserName = "otheruser" };
            var note = Note.Create(job, "Content", author);

            // Act & Assert
            Should.Throw<UnauthorizedAccessException>(() =>
                Note.Delete(note, differentUser)
            );
        }
    }
}
