﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SortMyStuffAPI.Models;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using SortMyStuffAPI.Services;

namespace SortMyStuffAPI.Controllers
{
    [Route("/[controller]")]
    [ApiVersion("0.1")]
    public class DetailsController 
        : EntityResourceBaseController<Detail, DetailEntity>
    {
        public DetailsController(
            IDetailDataService detailDataService, 
            IOptions<PagingOptions> defaultPagingOptions, 
            IOptions<ApiConfigs> apiConfigs, 
            IUserService userService, 
            IHostingEnvironment env, 
            IAuthorizationService authService) 
            : base(detailDataService, 
                  defaultPagingOptions, 
                  apiConfigs,
                  userService, 
                  env, 
                  authService)
        {
        }

        // GET /details
        [Authorize]
        [HttpGet(Name = nameof(GetDetailsAsync))]
        public async Task<IActionResult> GetDetailsAsync(
            CancellationToken ct,
            [FromQuery] PagingOptions pagingOptions,
            [FromQuery] SortOptions<Detail, DetailEntity> sortOptions,
            [FromQuery] SearchOptions<Detail, DetailEntity> searchOptions)
        {
            var errorMsg = "GET details failed.";
            if (!ModelState.IsValid) return BadRequest(
                new ApiError(errorMsg, ModelState));

            var result = await GetResourcesAsync(
                nameof(GetDetailsAsync),
                ct,
                pagingOptions,
                sortOptions,
                searchOptions);

            return GetActionResult(result, errorMsg);
        }

        // GET /details/{detailId}
        [Authorize]
        [HttpGet("{detailId}", Name = nameof(GetDetailByIdAsync))]
        public async Task<IActionResult> GetDetailByIdAsync(
            string detailId, CancellationToken ct)
        {
            return GetActionResult(
                await GetResourceByIdAsync(detailId, ct));
        }

        // CREATE resource is not supported by this method.
        // PATCH /details/{detailId}
        [Authorize]
        [HttpPatch("{detailId}", Name = nameof(UpdateDetailAsync))]
        public async Task<IActionResult> UpdateDetailAsync(
            string detailId,
            [FromBody] DetailForm updateForm,
            CancellationToken ct)
        {
            var errorMsg = "PATCH detail failed.";
            if (!ModelState.IsValid) return BadRequest(new ApiError(
                errorMsg, ModelState));

            var userId = await GetUserId();

            var record = await DataService.GetResourceAsync(
                userId, detailId, ct);
            if(record == null)
            {
                return NotFound(new ApiError(
                    errorMsg, "The detail does not exist."));
            }

            return GetActionResult(await UpdateResourceAsync(record, updateForm, ct));
        }
    }
}
