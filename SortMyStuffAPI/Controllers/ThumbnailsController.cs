using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SortMyStuffAPI.Models;
using SortMyStuffAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace SortMyStuffAPI.Controllers
{
    [Route("/[controller]")]
    [ApiVersion("0.1")]
    public class ThumbnailsController : ApiBaseController
    {
        private readonly IThumbnailFileService _thumbnailService;
        private readonly IAssetDataService _assetDataService;

        public ThumbnailsController(
            IUserDataService userDataService, 
            IOptions<ApiConfigs> apiConfigs, 
            IHostingEnvironment env, 
            IAuthorizationService authService,
            IThumbnailFileService thumbnailService,
            IAssetDataService assetService) 
            : base(userDataService, 
                  apiConfigs, 
                  env, 
                  authService)
        {
            _thumbnailService = thumbnailService;
            _assetDataService = assetService;
        }

        // GET /thumbnails/{assetId}.jpg
        [Authorize]
        [HttpGet("{assetId}.jpg", Name = nameof(GetThumbnailByIdAsync))]
        public async Task<IActionResult> GetThumbnailByIdAsync(string assetId, CancellationToken ct)
        {
            var userId = await GetUserId();

            if (await _assetDataService.GetResourceAsync(userId, assetId, ct) == null)
                return NotFound(new ApiError("Asset id not found."));

            var stream = await _thumbnailService.DownloadThumbnail(userId, assetId, ct);

            if (stream == null)
                return BadRequest(new ApiError("Downloading image file failed."));

            var response = File(stream, "application/octet-stream");
            return response;
        }
    }
}
