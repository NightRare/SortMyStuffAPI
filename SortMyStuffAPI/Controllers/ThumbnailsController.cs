using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SortMyStuffAPI.Models;
using SortMyStuffAPI.Services;
using Microsoft.AspNetCore.Authorization;

namespace SortMyStuffAPI.Controllers
{
    [Route("/[controller]")]
    [ApiVersion("0.1")]
    public class ThumbnailsController : Controller
    {
        private readonly IThumbnailFileService _thumbnailService;
        private readonly IAssetDataService _assetDataService;

        public ThumbnailsController(
            IThumbnailFileService thumbnailService, 
            IAssetDataService assetDataService)
        {
            _thumbnailService = thumbnailService;
            _assetDataService = assetDataService;
        }

        // GET /thumbnails/{assetId}.jpg
        [Authorize]
        [HttpGet("{assetId}.jpg", Name = nameof(GetThumbnailByIdAsync))]
        public async Task<IActionResult> GetThumbnailByIdAsync(string assetId, CancellationToken ct)
        {
            if (await _assetDataService.GetResourceAsync(assetId, ct) == null)
                return NotFound(new ApiError("Asset id not found."));

            var stream = await _thumbnailService.DownloadThumbnail(assetId, ct);

            if (stream == null)
                return BadRequest(new ApiError("Downloading image file failed."));

            var response = File(stream, "application/octet-stream");
            return response;
        }
    }
}
