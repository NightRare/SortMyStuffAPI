using System.Threading;
using System.Threading.Tasks;
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
    public class AssetTreesController : ApiBaseController
    {
        private readonly IAssetDataService _assetDataService;

        public AssetTreesController(
            IAssetDataService assetDataService, 
            IOptions<ApiConfigs> apiConfigs,
            IUserDataService userDataService,
            IHostingEnvironment env,
            IAuthorizationService authService)
            : base(userDataService,
                  apiConfigs,
                  env,
                  authService)
        {
            _assetDataService = assetDataService;
        }

        // GET /assettrees/{assetId}
        [Authorize]
        [HttpGet("{assetId}", Name = nameof(GetAssetTreeByIdAsync))]
        public async Task<IActionResult> GetAssetTreeByIdAsync(string assetId, CancellationToken ct)
        {
            string userId = await GetUserId();

            var assetTree = await _assetDataService.GetAssetTreeAsync(userId, assetId, ct);
            if (assetTree == null) return NotFound();

            return Ok(assetTree);
        }

    }

}
