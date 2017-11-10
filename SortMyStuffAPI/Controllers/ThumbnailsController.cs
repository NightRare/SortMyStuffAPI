using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SortMyStuffAPI.Services;

namespace SortMyStuffAPI.Controllers
{
    [Route("/[controller]")]
    [ApiVersion("0.1")]
    public class ThumbnailsController : Controller
    {
        private readonly IThumbnailFileService _thumbnailService;

        public ThumbnailsController(IThumbnailFileService thumbnailService)
        {
            _thumbnailService = thumbnailService;
        }

        // GET /thumbnails/{assetId}
        [HttpGet("{assetId}", Name = nameof(GetThumbnailById))]
        public async Task<IActionResult> GetThumbnailById(string assetId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

    }
}
