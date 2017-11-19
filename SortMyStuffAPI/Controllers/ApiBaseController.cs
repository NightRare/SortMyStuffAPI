using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SortMyStuffAPI.Exceptions;
using SortMyStuffAPI.Infrastructure;
using SortMyStuffAPI.Models;
using SortMyStuffAPI.Services;
using SortMyStuffAPI.Utils;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SortMyStuffAPI.Controllers
{
    public abstract class ApiBaseController : Controller
    {
        private readonly static string _developerUid = ServicesAuthHelper.DeveloperUid;

        protected readonly IUserService UserService;
        protected readonly ApiConfigs ApiConfigs;
        protected readonly IHostingEnvironment Env;
        protected readonly IAuthorizationService AuthService;

        protected ApiBaseController(
            IUserService userService,
            IOptions<ApiConfigs> apiConfigs,
            IHostingEnvironment env,
            IAuthorizationService authService)
        {
            UserService = userService;
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

            // if the admin policy authorization approved, return the developer Id
            return adminPolicy.Succeeded ?
                _developerUid : await UserService.GetUserIdAsync(User);
        }

        protected IActionResult GetActionResult(
            IStatusCodeResult result,
            string errorMessage = "Operation failed.")
        {
            IActionResultFuncArguments args;
            if (result.Succeeded)
            {
                args = new IActionResultFuncArguments
                {
                    Value = result.ResultObject
                };
                if (result.StatusCode.Equals(HttpStatusCode.Created) &&
                    result.ResultObject is Resource)
                {
                    var createdObject = (Resource)result.ResultObject;
                    args.Uri = Url.Link(
                        createdObject.Self.RouteName,
                        createdObject.Self.RouteValues);
                }
            }
            else
            {
                args = new IActionResultFuncArguments
                {
                    Value = new ApiError(
                        errorMessage, result.ErrorMessage)
                };
            }

            return GetActionResultFunc(result.StatusCode)
                .Invoke(args);
        }

        protected IActionResult GetActionResult<TResult>(
            IStatusCodeResult<TResult> result,
            string errorMessage = "Operation failed.")
            where TResult : Resource
        {
            return GetActionResult(result.ToNonGeneric(), errorMessage);
        }

        #region PRIVATE METHODS

        private Func<IActionResultFuncArguments, IActionResult>
            GetActionResultFunc(HttpStatusCode statusCode)
        {
            switch (statusCode)
            {
                // successful
                case HttpStatusCode.OK:
                    return args => args?.Value == null ?
                        Ok() : Ok(args.Value) as IActionResult;

                case HttpStatusCode.Created:
                    return args => Created(args.Uri, args.Value);

                case HttpStatusCode.NoContent:
                    return args => NoContent();

                // failed
                case HttpStatusCode.BadRequest:
                    return args => args?.Value == null ?
                        BadRequest() : BadRequest(args?.Value)
                        as IActionResult;

                case HttpStatusCode.NotFound:
                    return args => args?.Value == null ?
                        NotFound() : NotFound(args?.Value)
                        as IActionResult;

                default:
                    throw new ApiException($"Map the code {statusCode} to a " +
                        $"corresponding IActionResult function before use.");
            }
        }

        private class IActionResultFuncArguments
        {
            public string Uri { get; set; }
            public object Value { get; set; }
        }

        #endregion

    }
}
