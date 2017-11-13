using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SortMyStuffAPI.Models;

namespace SortMyStuffAPI.Controllers
{
    [Route("/")]
    [ApiVersion("0.1")]
    public class RootController : Controller
    {
        [AllowAnonymous]
        [HttpGet(Name = nameof(GetRoot))]
        public IActionResult GetRoot()
        {
            var response = new RootResponse
            {
                Self = Link.To(nameof(GetRoot)),
                SignInToken = Link.To(nameof(TokenController.TokenExchangeAsync)),
                Me = Link.To(nameof(UsersController.GetMeAsync)),
                Documentations = Link.ToCollection(nameof(DocsController.GetDocs)),
                Categories = Link.ToCollection(nameof(CategoriesController.GetCategoriesAsync)),
                AssetTrees = Link.ToCollection(nameof(AssetTreesController.GetAssetTreesAsync)),
                Assets = Link.ToCollection(nameof(AssetsController.GetAssetsAsync)),
                NewGuid = Link.To(nameof(NewGuidController.GetNewGuid)),
                Users = Link.ToCollection(nameof(UsersController.GetUsersAsync))
            };

            return Ok(response);
        }
    }
}
