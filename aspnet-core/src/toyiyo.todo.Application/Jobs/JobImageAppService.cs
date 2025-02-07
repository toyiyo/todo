using System;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using toyiyo.todo.Authorization;

namespace toyiyo.todo.Jobs
{
    [AbpAuthorize(PermissionNames.Pages_Jobs)]
    public class JobImageAppService : todoAppServiceBase, IJobImageAppService
    {
        private readonly IJobImageManager _jobImageManager;
        private readonly IJobManager _jobManager;

        public JobImageAppService(
            IJobImageManager jobImageManager,
            IJobManager jobManager)
        {
            _jobImageManager = jobImageManager;
            _jobManager = jobManager;
        }

        public async Task<JobImageDto> Create(JobImageCreateInputDto input)
        {
            var tenant = await GetCurrentTenantAsync();
            var job = await _jobManager.Get(input.JobId);
            var user = await GetCurrentUserAsync();

            using(var ms = new System.IO.MemoryStream())
            {
                await input.ImageData.CopyToAsync(ms);
                var fileBytes = ms.ToArray();
                var contentHash = JobImage.ComputeHash(fileBytes);

                // Check if image already exists
                var existingImage = await _jobImageManager.GetByHash(contentHash);
                if (existingImage != null)
                {
                    return ObjectMapper.Map<JobImageDto>(existingImage);
                }

                var jobImage = JobImage.Create(
                    job,
                    input.ContentType,
                    input.FileName,
                    fileBytes,
                    tenant.Id,
                    user
                );

                await _jobImageManager.Create(jobImage);
                return ObjectMapper.Map<JobImageDto>(jobImage);
            }
        }

        public async Task<JobImageDto> Get(Guid id)
        {
            try
            {
                var image = await _jobImageManager.Get(id);
                if (image == null)
                {
                    throw new Abp.Domain.Entities.EntityNotFoundException(typeof(JobImage), id);
                }

                return ObjectMapper.Map<JobImageDto>(image);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error retrieving image with ID {id}", ex);
                throw new UserFriendlyException("Could not retrieve the image. Please try again later.");
            }
        }

        public async Task<IActionResult> GetImage(Guid id)
        {
            try
            {
                var image = await _jobImageManager.Get(id);
                if (image == null)
                {
                    return new NotFoundResult();
                }

                return new FileContentResult(image.ImageData, image.ContentType)
                {
                    FileDownloadName = image.FileName
                };
            }
            catch (Exception ex)
            {
                Logger.Error($"Error retrieving image with ID {id}", ex);
                return new StatusCodeResult(500); // Internal Server Error
            }
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _jobImageManager.Delete(id, await GetCurrentUserAsync());
                return new NoContentResult();
            }
            catch (ArgumentNullException)
            {
                return new NotFoundResult();
            }
        }

    }
}