using Xunit;
using toyiyo.todo.Jobs;
using toyiyo.todo.Authorization.Users;
using System.Threading.Tasks;
using toyiyo.todo.Projects;
using System;

namespace toyiyo.todo.Tests.Jobs
{
    public class JobImageTests : todoTestBase
    {
        private readonly IJobAppService _jobAppService;
        private readonly IProjectAppService _projectAppService;

        public JobImageTests()
        {
            _jobAppService = Resolve<IJobAppService>();
            _projectAppService = Resolve<IProjectAppService>();
            LoginAsDefaultTenantAdmin();
        }
        
        [Fact]
        public async Task Should_Create_JobImage()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = Project.Create("test", currentUser, currentTenant.Id);
            var job = Job.Create(project, "test job", "test job", currentUser, currentTenant.Id);

            var contentType = "image/png";
            var fileName = "test.png";
            var imageData = new byte[] { 1, 2, 3, 4, 5 };
            var tenantId = 1;

            // Act
            var jobImage = JobImage.Create(job, contentType, fileName, imageData, tenantId, currentUser);

            // Assert
            Assert.NotNull(jobImage);
            Assert.Equal(job.Id, jobImage.JobId);
            Assert.Equal(contentType, jobImage.ContentType);
            Assert.Equal(fileName, jobImage.FileName);
            Assert.Equal(imageData, jobImage.ImageData);
            Assert.Equal(tenantId, jobImage.TenantId);
            Assert.Equal(currentUser.Id, jobImage.CreatorUserId);
            Assert.Equal(currentUser.Id, jobImage.LastModifierUserId);
            Assert.False(string.IsNullOrEmpty(jobImage.ContentHash));
        }

        [Fact]
        public async Task  Should_Set_ImageData()
        {
            // Arrange
            var jobImage = new JobImage();
            var imageData = new byte[] { 1, 2, 3, 4, 5 };
            var user = new User { Id = 1 };

            // Act
            JobImage.SetImageData(jobImage, imageData, user);

            // Assert
            Assert.Equal(imageData, jobImage.ImageData);
            Assert.Equal(user.Id, jobImage.LastModifierUserId);
        }

        [Fact]
        public void Should_Delete_JobImage()
        {
            // Arrange
            var jobImage = new JobImage();
            var user = new User { Id = 1 };

            // Act
            JobImage.Delete(jobImage, user);

            // Assert
            Assert.True(jobImage.IsDeleted);
            Assert.Equal(user.Id, jobImage.DeleterUserId);
        }

        [Fact]
        public void Should_Compute_Hash()
        {
            // Arrange
            var imageData = new byte[] { 1, 2, 3, 4, 5 };

            // Act
            var hash = JobImage.ComputeHash(imageData);

            // Assert
            Assert.False(string.IsNullOrEmpty(hash));
            Assert.Equal(64, hash.Length); // SHA-256 hash length in hexadecimal
        }

        [Fact]
        public void Should_Throw_Exception_When_Job_Is_Null()
        {
            // Arrange
            Job job = null;
            var contentType = "image/png";
            var fileName = "test.png";
            var imageData = new byte[] { 1, 2, 3, 4, 5 };
            var tenantId = 1;
            var user = new User { Id = 1 };

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => JobImage.Create(job, contentType, fileName, imageData, tenantId, user));
        }

        [Fact]
        public async Task Should_Throw_Exception_When_ImageData_Is_NullAsync()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = Project.Create("test", currentUser, currentTenant.Id);
            var job = Job.Create(project, "test job", "test job", currentUser, currentTenant.Id);
            var contentType = "image/png";
            var fileName = "test.png";
            byte[] imageData = null;
            var tenantId = 1;
            var user = new User { Id = 1 };

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => JobImage.Create(job, contentType, fileName, imageData, tenantId, user));
        }

        [Fact]
        public async Task Should_Throw_Exception_When_User_Is_NullAsync()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = Project.Create("test", currentUser, currentTenant.Id);
            var job = Job.Create(project, "test job", "test job", currentUser, currentTenant.Id);
            var contentType = "image/png";
            var fileName = "test.png";
            var imageData = new byte[] { 1, 2, 3, 4, 5 };
            var tenantId = 1;
            User user = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => JobImage.Create(job, contentType, fileName, imageData, tenantId, user));
        }

        [Fact]
        public async Task Should_Throw_Exception_When_TenantId_MismatchAsync()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = Project.Create("test", currentUser, currentTenant.Id);
            var job = Job.Create(project, "test job", "test job", currentUser, currentTenant.Id);
            var contentType = "image/png";
            var fileName = "test.png";
            var imageData = new byte[] { 1, 2, 3, 4, 5 };
            var tenantId = 100;
            var user = new User { Id = 1 };

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => JobImage.Create(job, contentType, fileName, imageData, tenantId, user));
        }
    }
}