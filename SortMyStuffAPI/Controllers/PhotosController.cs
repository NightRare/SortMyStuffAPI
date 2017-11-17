using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SortMyStuffAPI.Models;
using SortMyStuffAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;

namespace SortMyStuffAPI.Controllers
{
    [Route("/[controller]")]
    [ApiVersion("0.1")]
    public class PhotosController : ApiBaseController
    {
        private readonly IPhotoFileService _photoService;
        private readonly IAssetDataService _assetDataService;

        public PhotosController(
            IPhotoFileService photoService,
            IOptions<ApiConfigs> apiConfigs,
            IAssetDataService assetDataService,
            IUserDataService userDataService,
            IHostingEnvironment env,
            IAuthorizationService authService)
            : base(userDataService,
                  apiConfigs,
                  env,
                  authService)
        {
            _photoService = photoService;
            _assetDataService = assetDataService;
        }

        // GET /photos/{assetId}.jpg
        [Authorize]
        [HttpGet("{assetId}.jpg", Name = nameof(GetPhotoByIdAsync))]
        public async Task<IActionResult> GetPhotoByIdAsync(string assetId, CancellationToken ct)
        {
            var errorMsg = "Download photo failed.";

            var userId = await GetUserId();

            if (await _assetDataService.GetResourceAsync(userId, assetId, ct) == null)
                return NotFound(new ApiError(errorMsg, "Asset id not found."));

            var stream = await _photoService.DownloadPhoto(userId, assetId, ct);

            if (stream == null)
                return BadRequest(new ApiError(errorMsg, "Downloading image file failed."));

            var response = File(stream, "application/octet-stream");
            return response;
        }

        // PUT /photos/{assetId}.jpg
        [Authorize]
        [HttpPut("{assetId}.jpg", Name = nameof(UploadPhotoAsync))]
        public async Task<IActionResult> UploadPhotoAsync(
            string assetId,
            IFormFile photo,
            CancellationToken ct)
        {
            var errorMsg = "Upload photo failed.";

            var userId = await GetUserId();

            if (await _assetDataService.GetResourceAsync(userId, assetId, ct) == null)
            {
                return NotFound(new ApiError(errorMsg, "Asset id not found."));
            }

            if (photo == null)
            {
                return BadRequest(new ApiError(errorMsg, "No file uploaded."));
            }

            if (photo.Length > ApiConfigs.ImageMaxBytes)
            {
                return BadRequest(
                    new ApiError(errorMsg,
                    $"File over size.(limit: {ApiConfigs.ImageMaxBytes / 1024 / 1024} mb)"));
            }

            var fileName = Path.GetTempFileName();
            using (var photoFile = new FileStream(fileName, FileMode.Create))
            {
                await photo.CopyToAsync(photoFile, ct);
                var uploadTask = await _photoService.UploadPhoto(userId, assetId, photoFile, ct);

                if (!uploadTask.Succeeded)
                {
                    return BadRequest(new ApiError(errorMsg, uploadTask.Error));
                }
                return Ok();
            }
        }

        // DELETE /photos/{assetId}.jpg
        [HttpDelete("{assetId}.jpg", Name = nameof(DeletePhotoAsync))]
        public async Task<IActionResult> DeletePhotoAsync(
            string assetId,
            CancellationToken ct)
        {
            var errorMsg = "Delete photo failed.";

            var userId = await GetUserId();

            var asset = await _assetDataService.GetResourceAsync(userId, assetId, ct);
            if(asset == null)
            {
                return NotFound(new ApiError(errorMsg, "Asset id not found."));
            }

            var deleteTask = await _photoService.DeletePhoto(asset.UserId, assetId, ct);
            if(!deleteTask.Succeeded)
            {
                if (deleteTask.Error.Contains("404"))
                {
                    return NotFound(new ApiError(errorMsg, "Photo file not found"));
                }
                return BadRequest(new ApiError(errorMsg, deleteTask.Error));
            }

            return NoContent();
        }
    }
}
