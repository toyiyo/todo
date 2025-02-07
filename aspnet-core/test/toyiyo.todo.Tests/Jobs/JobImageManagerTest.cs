using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using toyiyo.todo.Authorization.Users;
using toyiyo.todo.Jobs;
using toyiyo.todo.Projects;
using Xunit;

namespace toyiyo.todo.Tests.Jobs
{
    public class JobImageManagerTest : todoTestBase
    {
        private readonly IJobImageManager _jobImageManager;

        public JobImageManagerTest()
        {
            _jobImageManager = Resolve<IJobImageManager>();
            LoginAsDefaultTenantAdmin();
        }

        [Fact]
        public async Task Get_ShouldReturnJobImage()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = Project.Create("test", currentUser, currentTenant.Id);
            var job = Job.Create(project, "test job", "test job", currentUser, currentTenant.Id);
            var contentType = "image/png";
            var fileName = "test.png";
            var imageData = new byte[] { 1, 2, 3, 4, 5 };
            var tenantId = currentTenant.Id;

            var jobImage = JobImage.Create(job, contentType, fileName, imageData, tenantId, currentUser);

            // Act
            var Image = await _jobImageManager.Create(jobImage);
            // Act
            var result = await _jobImageManager.Get(Image.Id);

            // Assert
            Assert.Equal(jobImage.ImageData, result.ImageData);
        }

        [Fact]
        public async Task Create_ShouldInsertJobImage()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = Project.Create("test", currentUser, currentTenant.Id);
            var job = Job.Create(project, "test job", "test job", currentUser, currentTenant.Id);
            var contentType = "image/png";
            var fileName = "test.png";
            var imageData = new byte[] { 1, 2, 3, 4, 5 };
            var tenantId = currentTenant.Id;

            var jobImage = JobImage.Create(job, contentType, fileName, imageData, tenantId, currentUser);

            // Act
            var result = await _jobImageManager.Create(jobImage);

            // Assert
            Assert.Equal(jobImage, result);
        }

        [Fact]
        public async Task Delete_ShouldRemoveJobImage()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = Project.Create("test", currentUser, currentTenant.Id);
            var job = Job.Create(project, "test job", "test job", currentUser, currentTenant.Id);
            var contentType = "image/png";
            var fileName = "test.png";
            var imageData = new byte[] { 1, 2, 3, 4, 5 };
            var tenantId = currentTenant.Id;

            var jobImage = JobImage.Create(job, contentType, fileName, imageData, tenantId, currentUser);
            var image = await _jobImageManager.Create(jobImage);

            // Act
            await _jobImageManager.Delete(image.Id, currentUser);

            // Assert
            await Assert.ThrowsAsync<Abp.Domain.Entities.EntityNotFoundException>(async () => await _jobImageManager.Get(image.Id));
        }

        [Fact]
        public async Task GetByJobId_ShouldReturnJobImages()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = Project.Create("test", currentUser, currentTenant.Id);
            var job = Job.Create(project, "test job", "test job", currentUser, currentTenant.Id);
            await UsingDbContextAsync(async context =>
            {
                var jobImages = new List<JobImage>
                {
                    JobImage.Create(job, "image/png", "test1.png", new byte[] { 1, 2, 3 }, 1, new User { Id = 1 }),
                    JobImage.Create(job,"image/jpeg", "test2.jpg", new byte[] { 4, 5, 6 }, 1, new User { Id = 1 })
                };
                await context.JobImages.AddRangeAsync(jobImages);
                await context.SaveChangesAsync();
            });

            // Act
            var result = await _jobImageManager.GetByJobId(job.Id);

            // Assert
            Assert.Equal(2, result.Count); 
        }

        [Fact]
        public async Task GetByHash_ShouldReturnJobImage()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = Project.Create("test", currentUser, currentTenant.Id);
            var job = Job.Create(project, "test job", "test job", currentUser, currentTenant.Id);
            var contentType = "image/png";
            var fileName = "test.png";
            var imageData = new byte[] { 1, 2, 3, 4, 5 };
            var tenantId = currentTenant.Id;

            var jobImage = JobImage.Create(job, contentType, fileName, imageData, tenantId, currentUser);

            var image = await _jobImageManager.Create(jobImage);
            // Act
            var result = await _jobImageManager.GetByHash(image.ContentHash);

            // Assert
            Assert.Equal(jobImage.ImageData, result.ImageData);
        }
    }
}