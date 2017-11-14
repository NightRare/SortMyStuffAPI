using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SortMyStuffAPI.Models;
using SortMyStuffAPI.Services;
using SortMyStuffAPI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SortMyStuffAPI.Controllers
{
    public abstract class ApiBaseController : Controller
    {
        private readonly static string _developerUid = ServicesAuthHelper.DeveloperUid;

        protected readonly IUserDataService UserService;
        protected readonly ApiConfigs ApiConfigs;
        protected readonly IHostingEnvironment Env;
        protected readonly IAuthorizationService AuthService;

        protected ApiBaseController(
            IUserDataService userDataService,
            IOptions<ApiConfigs> apiConfigs,
            IHostingEnvironment env,
            IAuthorizationService authService)
        {
            UserService = userDataService;
            ApiConfigs = apiConfigs.Value;
            Env = env;
            AuthService = authService;
        }

        // if the userId is null, then it is able to get, update and delete
        // resources that belong to any user
        protected virtual async Task<string> GetUserId()
        {
            // if is in development and allow anonymous, then always apply the developer priviledge
            if (Env.IsDevelopment() && ApiConfigs.AllowAnonymous)
            {
                return _developerUid;
            }

            var adminPolicy = await AuthService
                .AuthorizeAsync(User, ApiStrings.PolicyDeveloper);

            // if the admin policy authorization approved, userId is null
            return adminPolicy.Succeeded ?
                _developerUid : await UserService.GetUserIdAsync(User);
        }
    }
}
