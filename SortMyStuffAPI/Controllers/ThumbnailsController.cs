using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SortMyStuffAPI.Controllers
{
    [Route("/[controller]")]
    [ApiVersion("0.1")]
    public class ThumbnailsController : Controller
    {
        [HttpGet("{assetId}", Name = nameof(GetThumbnailById))]
        public async Task<IActionResult> GetThumbnailById(string assetId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
