using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using toyiyo.todo.Jobs;
using toyiyo.todo.Projects;
using Xunit;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.ComponentModel.DataAnnotations;

namespace toyiyo.todo.Tests.Jobs
{
    public class JobImageAppServiceTest : todoTestBase
    {
        private readonly IJobImageAppService _jobImageAppService;
        private readonly IJobAppService _jobAppService;
        private readonly IProjectAppService _projectAppService;

        public JobImageAppServiceTest()
        {
            _jobImageAppService = Resolve<IJobImageAppService>();
            _jobAppService = Resolve<IJobAppService>();
            _projectAppService = Resolve<IProjectAppService>();
            LoginAsDefaultTenantAdmin();
        }

        private IFormFile CreateFormFile(byte[] fileBytes, string fileName, string contentType)
        {
            var stream = new MemoryStream(fileBytes);
            return new FormFile(stream, 0, fileBytes.Length, fileName, fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }

        [Fact]
        public async Task Create_ShouldReturnJobImageDto()
        {
            var projectDto = await _projectAppService.Create(new CreateProjectInputDto { Title = "test project" });
            var jobDto = await _jobAppService.Create(new JobCreateInputDto
            {
                ProjectId = projectDto.Id,
                Title = "Test Job",
                Description = "Test Description"
            });

            var input = new JobImageCreateInputDto
            {
                JobId = jobDto.Id,
                ContentType = "image/jpeg",
                FileName = "test.jpg",
                ImageData = CreateFormFile(new byte[] { 1, 2, 3 }, "test.jpg", "image/jpeg")
            };

            ValidateObject(input);

            var result = await _jobImageAppService.Create(input);

            Assert.NotNull(result);
            Assert.Equal(input.ContentType, result.ContentType);
            Assert.Equal(input.FileName, result.FileName);
        }

        [Fact]
        public async Task Get_ShouldReturnJobImageDto()
        {
            var projectDto = await _projectAppService.Create(new CreateProjectInputDto { Title = "another project" });
            var jobDto = await _jobAppService.Create(new JobCreateInputDto
            {
                ProjectId = projectDto.Id,
                Title = "Another Job",
                Description = "Another desc"
            });

            var created = await _jobImageAppService.Create(new JobImageCreateInputDto
            {
                JobId = jobDto.Id,
                ContentType = "image/png",
                FileName = "test-get.png",
                ImageData = CreateFormFile(new byte[] { 9, 9, 9 }, "test-get.png", "image/png")
            });

            ValidateObject(created);

            var result = await _jobImageAppService.Get(created.Id);

            Assert.NotNull(result);  
            Assert.Equal(created.Id, result.Id);  
            Assert.Equal(created.JobId, result.JobId);  
            Assert.Equal(created.ContentType, result.ContentType);  
            Assert.Equal(created.FileName, result.FileName);  
            Assert.Equal(created.imageUrl, result.imageUrl); 
        }

        [Fact]
        public async Task GetImage_ShouldReturnFileContentResult()
        {
            var projectDto = await _projectAppService.Create(new CreateProjectInputDto { Title = "get-image project" });
            var jobDto = await _jobAppService.Create(new JobCreateInputDto
            {
                ProjectId = projectDto.Id,
                Title = "GetImageTest",
                Description = "desc"
            });

            var created = await _jobImageAppService.Create(new JobImageCreateInputDto
            {
                JobId = jobDto.Id,
                ContentType = "image/png",
                FileName = "test-getimage.png",
                ImageData = CreateFormFile(new byte[] { 1, 2, 3, 4 }, "test-getimage.png", "image/png")
            });

            ValidateObject(created);

            var actionResult = await _jobImageAppService.GetImage(created.Id);
            var fileResult = Assert.IsType<FileContentResult>(actionResult);

            Assert.Equal(created.ContentType, fileResult.ContentType);
            Assert.Equal(created.FileName, fileResult.FileDownloadName);
        }

        [Fact]
        public async Task Delete_ShouldReturnNoContentResult()
        {
            var projectDto = await _projectAppService.Create(new CreateProjectInputDto { Title = "delete project" });
            var jobDto = await _jobAppService.Create(new JobCreateInputDto
            {
                ProjectId = projectDto.Id,
                Title = "DeleteTest",
                Description = "desc"
            });

            var created = await _jobImageAppService.Create(new JobImageCreateInputDto
            {
                JobId = jobDto.Id,
                ContentType = "image/png",
                FileName = "test-delete.png",
                ImageData = CreateFormFile(new byte[] { 5, 6, 7 }, "test-delete.png", "image/png")
            });

            ValidateObject(created);

            var result = await _jobImageAppService.Delete(created.Id);
            Assert.IsType<NoContentResult>(result);

            await Assert.ThrowsAsync<Abp.UI.UserFriendlyException>(
                async () => await _jobImageAppService.Get(created.Id)
            );
        }

        private void ValidateObject(object obj)
        {
            var validationContext = new ValidationContext(obj, null, null);
            Validator.ValidateObject(obj, validationContext, validateAllProperties: true);
        }
    }
}