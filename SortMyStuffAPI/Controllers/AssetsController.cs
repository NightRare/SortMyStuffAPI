using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SortMyStuffAPI.Services;
using SortMyStuffAPI.Utils;

namespace SortMyStuffAPI.Controllers
{
    [Route("/[controller]")]
    [ApiVersion("0.1")]
    public class AssetsController : Controller
    {

        private readonly IAssetDataService _ads;

        public AssetsController(IAssetDataService assetDataService)
        {
            _ads = assetDataService;
        }

        [HttpGet(Name = nameof(GetAssets))]
        public async Task<IActionResult> GetAssets()
        {
            var response = new
            {
                href = Url.Link(nameof(GetAssets), null),
                resource = await _ads.GetAssets(CancellationToken.None)
            };

            return Ok(response);
        }

    }
}
