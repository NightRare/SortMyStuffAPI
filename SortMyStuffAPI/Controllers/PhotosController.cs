using System;
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
using SortMyStuffAPI.Infrastructure;

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
            var userId = await GetUserId();

            if (await _assetDataService.GetResourceAsync(userId, assetId, ct) == null)
                return NotFound(new ApiError("Asset id not found."));

            var stream = await _photoService.DownloadPhoto(userId, assetId, ct);

            if (stream == null)
                return BadRequest(new ApiError("Downloading image file failed."));

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
            var userId = await GetUserId();

            if (await _assetDataService.GetResourceAsync(userId, assetId, ct) == null)
                return NotFound(new ApiError("Asset id not found."));

            if (photo == null)
                return BadRequest(new ApiError("No file uploaded."));

            if (photo.Length > ApiConfigs.ImageMaxBytes)
                return BadRequest(
                    new ApiError($"File over size.(limit: {ApiConfigs.ImageMaxBytes / 1024 / 1024} mb)"));

            var fileName = Path.GetTempFileName();
            using (var photoFile = new FileStream(fileName, FileMode.Create))
            {
                await photo.CopyToAsync(photoFile, ct);
                bool uploaded = await _photoService.UploadPhoto(userId, assetId, photoFile, ct);

                if (uploaded) return Ok();
            }

            return BadRequest(new ApiError("Uploading image file failed."));
        }

        // DELETE /photos/{assetId}.jpg
        [HttpDelete("{assetId}.jpg", Name = nameof(DeletePhotoAsync))]
        public async Task<IActionResult> DeletePhotoAsync(
            string assetId,
            CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
