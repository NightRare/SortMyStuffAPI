using Microsoft.AspNetCore.Mvc;
using SortMyStuffAPI.Models;

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
                Documentations = Link.To(nameof(DocsController.GetDocs)),
                AssetTrees = Link.To(nameof(AssetTreesController.GetAssetTreeAsync)),
                Assets = Link.To(nameof(AssetsController.GetAssetsAsync)),
                NewGuid = Link.To(nameof(NewGuidController.GetNewGuid))
            };

            return Ok(response);
        }
    }
}
