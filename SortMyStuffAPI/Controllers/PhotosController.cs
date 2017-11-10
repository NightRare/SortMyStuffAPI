using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SortMyStuffAPI.Models;
using SortMyStuffAPI.Services;

namespace SortMyStuffAPI.Controllers
{
    [Route("/[controller]")]
    [ApiVersion("0.1")]
    public class PhotosController : Controller
    {
        private readonly IPhotoFileService _photoService;
        private readonly IAssetDataService _assetDataService;
        private readonly ApiConfigs _apiConfigs;

        public PhotosController(
            IPhotoFileService photoService,
            IOptions<ApiConfigs> apiConfigs,
            IAssetDataService assetDataService)
        {
            _photoService = photoService;
            _assetDataService = assetDataService;
            _apiConfigs = apiConfigs.Value;
        }

        // GET /photos/{assetId}.jpg
        [HttpGet("{assetId}.jpg", Name = nameof(GetPhotoByIdAsync))]
        public async Task<IActionResult> GetPhotoByIdAsync(string assetId, CancellationToken ct)
        {
            if (await _assetDataService.GetAssetAsync(assetId, ct) == null)
                return NotFound(new ApiError("Asset id not found."));

            var stream = await _photoService.DownloadPhoto(assetId, ct);

            if (stream == null)
                return BadRequest(new ApiError("Downloading image file failed."));

            var response = File(stream, "application/octet-stream");
            return response;
        }

        // PUT /photos/{assetId}.jpg
        [HttpPut("{assetId}.jpg", Name = nameof(UploadPhotoAsync))]
        public async Task<IActionResult> UploadPhotoAsync(
            string assetId,
            IFormFile photo,
            CancellationToken ct)
        {
            if (await _assetDataService.GetAssetAsync(assetId, ct) == null)
                return NotFound(new ApiError("Asset id not found."));

            if (photo == null)
                return BadRequest(new ApiError("No file uploaded."));

            if (photo.Length > _apiConfigs.ImageMaxBytes)
                return BadRequest(
                    new ApiError($"File over size.(limit: {_apiConfigs.ImageMaxBytes / 1024 / 1024} mb)"));

            var fileName = Path.GetTempFileName();
            using (var photoFile = new FileStream(fileName, FileMode.Create))
            {
                await photo.CopyToAsync(photoFile, ct);
                bool uploaded = await _photoService.UploadPhoto(assetId, photoFile, ct);

                if (uploaded) return Ok();
            }

            return BadRequest(new ApiError("Uploading image file failed."));
        }
    }
}
