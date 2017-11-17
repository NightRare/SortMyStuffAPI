using SortMyStuffAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using SortMyStuffAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using SortMyStuffAPI.Exceptions;

namespace SortMyStuffAPI.Controllers
{
    [Route("/[controller]")]
    [ApiVersion("0.1")]
    public class BaseDetailsController
        : EntityResourceBaseController<BaseDetail, BaseDetailEntity>
    {
        private readonly ICategoryDataService _categoryDataService;
        private readonly IDetailDataService _detailDataService;

        public BaseDetailsController(
            IBaseDetailDataService baseDetailDataService,
            IOptions<PagingOptions> defaultPagingOptions,
            IOptions<ApiConfigs> apiConfigs,
            IUserDataService userDataService,
            IHostingEnvironment env,
            IAuthorizationService authService,
            ICategoryDataService categoryDataService,
            IDetailDataService detailDataService)
            : base(baseDetailDataService,
                  defaultPagingOptions,
                  apiConfigs,
                  userDataService,
                  env,
                  authService)
        {
            _categoryDataService = categoryDataService;
            _detailDataService = detailDataService;
        }

        // GET /basedetails
        [Authorize]
        [HttpGet(Name = nameof(GetBaseDetailsAsync))]
        public async Task<IActionResult> GetBaseDetailsAsync(
            CancellationToken ct,
            [FromQuery] PagingOptions pagingOptions,
            [FromQuery] SortOptions<BaseDetail, BaseDetailEntity> sortOptions,
            [FromQuery] SearchOptions<BaseDetail, BaseDetailEntity> searchOptions)
        {
            return await GetResourcesAsync(
                nameof(GetBaseDetailsAsync),
                ct,
                pagingOptions,
                sortOptions,
                searchOptions);
        }

        // GET /basedetails/{baseDetailId}
        [Authorize]
        [HttpGet("{baseDetailId}", Name = nameof(GetBaseDetailByIdAsync))]
        public async Task<IActionResult> GetBaseDetailByIdAsync(
            string baseDetailId, CancellationToken ct)
        {
            return await GetResourceByIdAsync(baseDetailId, ct);
        }

        // POST /basedetails
        [Authorize]
        [HttpPost(Name = nameof(CreateBaseDetailAsync))]
        public async Task<IActionResult> CreateBaseDetailAsync(
            [FromBody] BaseDetailForm body,
            CancellationToken ct)
        {
            var errorMsg = "POST base detail failed.";
            if (!ModelState.IsValid) return BadRequest(new ApiError(
                errorMsg, ModelState));

            string userId = await GetUserId();

            if (!Enum.TryParse<DetailStyle>(body.Style, out var style))
            {
                return BadRequest(new ApiError(errorMsg, "Incorrect value of 'style'."));
            }

            var category = _categoryDataService.GetResourceAsync(
                userId, body.CategoryId, ct);
            if (category == null)
            {
                return NotFound(new ApiError(errorMsg,
                    "The associated category does not exist."));
            }

            // TODO: add details to the categorised assets

            return await CreateResourceAsync(
                nameof(GetBaseDetailByIdAsync),
                body,
                ct);
        }

        // PUT /basedetails/{baseDetailId}
        [Authorize]
        [HttpPut("{baseDetailId}", Name = nameof(AddOrUpdateBaseDetailAsync))]
        public async Task<IActionResult> AddOrUpdateBaseDetailAsync(
            string baseDetailId,
            [FromBody] BaseDetailForm body,
            CancellationToken ct)
        {
            var errorMsg = "PUT base detail failed.";
            if (!ModelState.IsValid) return BadRequest(new ApiError(
                errorMsg, ModelState));

            var userId = await GetUserId();

            if (!Enum.TryParse<DetailStyle>(body.Style, out var style))
            {
                return BadRequest(new ApiError(errorMsg, "Incorrect value of 'style'."));
            }

            var category = _categoryDataService.GetResourceAsync(
                userId, body.CategoryId, ct);
            if (category == null)
            {
                return NotFound(new ApiError(errorMsg,
                    "The associated category does not exist."));
            }

            // TODO: add details to the categorised assets

            return await AddOrUpdateResourceAsync(baseDetailId, body, ct, errorMessage: errorMsg);

        }

        // DELETE /basedetails/{baseDetailId}
        [Authorize]
        [HttpDelete("{baseDetailId}", Name = nameof(DeleteBaseDetailByIdAsync))]
        public async Task<IActionResult> DeleteBaseDetailByIdAsync(
            string baseDetailId,
            [FromQuery] DeletingOptions deletingOptions,
            CancellationToken ct)
        {
            var errorMsg = "Delete base detail failed.";
            if (!ModelState.IsValid) return BadRequest(
                new ApiError(errorMsg, ModelState));

            var userId = await GetUserId();

            try
            {
                var result = await DataService.DeleteResourceAsync(
                    userId, baseDetailId, deletingOptions.DelDependents, ct);

                if (!result.Succeeded)
                {
                    return BadRequest(new ApiError(
                        errorMsg,
                        result.Error));
                }
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
