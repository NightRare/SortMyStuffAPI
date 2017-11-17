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
using System.Net;

namespace SortMyStuffAPI.Controllers
{
    [Route("/[controller]")]
    [ApiVersion("0.1")]
    public class BaseDetailsController
        : EntityResourceBaseController<BaseDetail, BaseDetailEntity>
    {
        private readonly ICategoryDataService _categoryDataService;
        private readonly IDetailDataService _detailDataService;
        private readonly IAssetDataService _assetDataService;

        public BaseDetailsController(
            IBaseDetailDataService baseDetailDataService,
            IOptions<PagingOptions> defaultPagingOptions,
            IOptions<ApiConfigs> apiConfigs,
            IUserDataService userDataService,
            IHostingEnvironment env,
            IAuthorizationService authService,
            ICategoryDataService categoryDataService,
            IDetailDataService detailDataService,
            IAssetDataService assetDataService)
            : base(baseDetailDataService,
                  defaultPagingOptions,
                  apiConfigs,
                  userDataService,
                  env,
                  authService)
        {
            _categoryDataService = categoryDataService;
            _detailDataService = detailDataService;
            _assetDataService = assetDataService;
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

            BaseDetail baseDetail = null;
            Action<BaseDetail> operation = bd => baseDetail = bd;

            var response = await CreateResourceAsync(
                nameof(GetBaseDetailByIdAsync),
                Guid.NewGuid().ToString(),
                body,
                ct,
                operation: operation);

            if ((response as ObjectResult).StatusCode
                .Equals(HttpStatusCode.Created))
            {
                var addDetails = await AddDetailsToAssets(userId, baseDetail, ct);
                if (!addDetails.Succeeded)
                {
                    return BadRequest(new ApiError(errorMsg, addDetails.Error));
                }
            }

            return response;
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

            var record = await DataService.GetResourceAsync(
                userId, baseDetailId, ct);

            if (record == null)
            {
                BaseDetail baseDetail = null;
                Action<BaseDetail> operation = bd => baseDetail = bd;

                var response = await CreateResourceAsync(
                    nameof(GetBaseDetailByIdAsync),
                    baseDetailId,
                    body,
                    ct,
                    operation: operation,
                    errorMessage: errorMsg);

                if ((response as ObjectResult).StatusCode
                    .Equals(HttpStatusCode.Created))
                {
                    // add derivative details to the categorised assets
                    // TODO: handle follow-up operation failure, should reverse
                    // the entire operation
                    var addDetails = await AddDetailsToAssets(userId, baseDetail, ct);
                    if (!addDetails.Succeeded)
                    {
                        return BadRequest(new ApiError(errorMsg, addDetails.Error));
                    }
                    return Ok();
                }

                // if the base detail not added successfully
                return response;
            }

            return await UpdateResourceAsync(
                record, body, ct, errorMessage: errorMsg);
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

        #region PRIVATE METHODS

        private async Task<(bool Succeeded, string Error)> AddDetailsToAssets(
            string userId,
            BaseDetail baseDetail,
            CancellationToken ct)
        {
            var currentTime = DateTimeOffset.UtcNow;

            var assets = await _assetDataService.GetAssetsByCategoryId(
                userId, baseDetail.CategoryId, ct);

            var details = assets.Select(asset =>
                new Detail
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    BaseId = baseDetail.Id,
                    AssetId = asset.Id,
                    ModifyTimestamp = currentTime,
                    Field = null
                });

            var addDetail = await _detailDataService.AddResourceCollectionAsync(
                userId, details.ToArray(), ct);

            if (!addDetail.Succeeded)
            {
                return (false,
                    $"Add derivative detail to assets failed.");
            }
            return (true, null);
        }

        #endregion
    }
}
