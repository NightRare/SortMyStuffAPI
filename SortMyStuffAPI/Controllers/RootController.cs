using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SortMyStuffAPI.Models;
using SortMyStuffAPI.Services;

namespace SortMyStuffAPI.Controllers
{
    [Route("/")]
    [ApiVersion("0.1")]
    public class RootController : ApiBaseController
    {
        public RootController(
            IUserDataService userDataService, 
            IOptions<ApiConfigs> apiConfigs, 
            IHostingEnvironment env, 
            IAuthorizationService authService)
            : base(userDataService, 
                  apiConfigs, 
                  env, 
                  authService)
        {
        }

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
                Assets = Link.ToCollection(nameof(AssetsController.GetAssetsAsync)),
                BaseDetails = Link.ToCollection(nameof(BaseDetailsController.GetBaseDetailsAsync)),
                NewGuid = Link.To(nameof(NewGuidController.GetNewGuid)),
                Users = Link.ToCollection(nameof(UsersController.GetUsersAsync))
            };

            return Ok(response);
        }
    }
}
