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

namespace SortMyStuffAPI.Controllers
{
    [Route("/[controller]")]
    [ApiVersion("0.1")]
    public class UsersController :
        JsonResourcesController<User, UserEntity>
    {
        private readonly IUserDataService _userDataService;
        private readonly IAuthorizationService _authorizationService;

        public UsersController(
            IUserDataService dataService,
            IOptions<PagingOptions> defaultPagingOptions,
            IOptions<ApiConfigs> apiConfigs,
            IAuthorizationService authorizationService)
            : base(dataService, defaultPagingOptions, apiConfigs)
        {
            _userDataService = dataService;
        }

        // GET /users
        [Authorize(Roles = ApiStrings.RoleAdmin)]
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

        // GET /users/me
        [Authorize]
        [HttpGet("me", Name = nameof(GetMeAsync))]
        public async Task<IActionResult> GetMeAsync(
            CancellationToken ct)
        {
            if (User == null) return BadRequest();

            var user = await _userDataService.GetUserAsync(User);
            if (user == null) return NotFound();

            return Ok(user);
        }

        // GET /users/{userId}
        [Authorize(Roles = ApiStrings.RoleAdmin)]
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

            var currentTime = DateTimeOffset.UtcNow;

            var user = Mapper.Map<RegisterForm, User>(body);
            user.Id = Guid.NewGuid().ToString();
            user.Provider = AuthProvider.Native.ToString();
            user.CreateTimestamp = currentTime;

            var result = await _userDataService.CreateUserAsync(user, ct);
            if (!result.Succeeded)
            {
                return BadRequest(new ApiError("Registration failed.", result.Error));
            }

            var userCreated = await _userDataService.GetResourceAsync(user.Id, ct);

            return Created(
                Url.Link(nameof(GetUserByIdAsync),
                    new { userId = user.Id }),
                userCreated);
        }
    }
}
