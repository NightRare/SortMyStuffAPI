using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SortMyStuffAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SortMyStuffAPI.Controllers
{
    [Route("/[controller]")]
    [ApiVersion("0.1")]
    public class DetailsController : Controller
    {
        [Authorize]
        [HttpGet(Name = nameof(GetDetailsAsync))]
        public async Task<IActionResult> GetDetailsAsync(
            CancellationToken ct,
            [FromQuery] PagingOptions pagingOptions,
            [FromQuery] SortOptions<Asset, AssetEntity> sortOptions,
            [FromQuery] SearchOptions<Asset, AssetEntity> searchOptions)
        {
            throw new NotImplementedException();
        }
    }
}
