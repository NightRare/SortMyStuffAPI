using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SortMyStuffAPI.Models;
using SortMyStuffAPI.Services;
using Microsoft.AspNetCore.Authorization;
using SortMyStuffAPI.Utils;
using Microsoft.AspNetCore.Hosting;

namespace SortMyStuffAPI.Controllers
{
    [Route("/[controller]")]
    [ApiVersion("0.1")]
    public class UsersController :
        EntityResourceBaseController<User, UserEntity>
    {
        public UsersController(
            IOptions<PagingOptions> defaultPagingOptions, 
            IOptions<ApiConfigs> apiConfigs, 
            IUserDataService userDataService, 
            IHostingEnvironment env, 
            IAuthorizationService authService) : 
            base(userDataService,
                defaultPagingOptions, 
                apiConfigs, 
                userDataService, 
                env, 
                authService)
        {
        }

        // GET /users/me
        [Authorize]
        [HttpGet("me", Name = nameof(GetMeAsync))]
        public async Task<IActionResult> GetMeAsync(
            CancellationToken ct)
        {
            if (User == null) return BadRequest();

            var user = await UserService.GetUserAsync(User);
            if (user == null) return NotFound();

            var adminPolicy = await AuthService
                .AuthorizeAsync(User, ApiStrings.PolicyDeveloper);

            // if it voliates the admin policy, then erase user id information
            if (!adminPolicy.Succeeded)
            {
                user.Self = Link.To(nameof(GetMeAsync));
                user.Id = null;
            }

            return Ok(user);
        }

        // GET /users
        [Authorize(Policy = ApiStrings.PolicyDeveloper)]
        [HttpGet(Name = nameof(GetUsersAsync))]
        public async Task<IActionResult> GetUsersAsync(
            CancellationToken ct,
            [FromQuery] PagingOptions pagingOptions,
            [FromQuery] SortOptions<User, UserEntity> sortOptions,
            [FromQuery] SearchOptions<User, UserEntity> searchOptions)
        {
            return await GetResourcesAsync(
                nameof(GetUsersAsync),
                ct,
                pagingOptions,
                sortOptions,
                searchOptions);
        }

        // GET /users/{userId}
        [Authorize(Policy = ApiStrings.PolicyDeveloper)]
        [HttpGet("{userId}", Name = nameof(GetUserByIdAsync))]
        public async Task<IActionResult> GetUserByIdAsync(
            string userId,
            CancellationToken ct)
        {
            return await GetResourceByIdAsync(userId, ct);
        }

        // POST /users
        [AllowAnonymous]
        [HttpPost(Name = nameof(RegisterUserAsync))]
        public async Task<IActionResult> RegisterUserAsync(
            [FromBody] RegisterForm body,
            CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiError(ModelState));

            var adminPolicy = await AuthService
                .AuthorizeAsync(User, ApiStrings.PolicyDeveloper);

            var currentTime = DateTimeOffset.UtcNow;

            var user = Mapper.Map<RegisterForm, User>(body);
            user.Id = Guid.NewGuid().ToString();
            user.Provider = AuthProvider.Native.ToString();
            user.CreateTimestamp = currentTime;

            var result = await UserService.CreateUserAsync(user, ct);
            if (!result.Succeeded)
            {
                return BadRequest(new ApiError("Registration failed.", result.Error));
            }

            var userCreated = await UserService.GetResourceAsync(null, user.Id, ct);

            // if it voliates the admin policy, then erase user id information
            if (!adminPolicy.Succeeded)
            {
                userCreated.Self = Link.To(nameof(GetMeAsync));
                userCreated.Id = null;
            }

            return Created(
                Url.Link(nameof(GetMeAsync), null),
                userCreated);
        }
    }
}
