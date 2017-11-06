using Microsoft.AspNetCore.Mvc;
using SortMyStuffAPI.Models;
using SortMyStuffAPI.Models.Resources;

namespace SortMyStuffAPI.Controllers
{
    [Route("/")]
    [ApiVersion("0.1")] 
    public class RootController : Controller
    {
        [HttpGet(Name = nameof(GetRoot))]
        public IActionResult GetRoot()
        {
            var response = new RootResponse
            {
                Self = Link.To(nameof(GetRoot)),
                AssetTrees = Link.To(nameof(AssetTreesController.GetAssetTreeAsync)),
                Assets = Link.To(nameof(AssetsController.GetAssetsAsync))
            };

            return Ok(response);
        }
    }
}
