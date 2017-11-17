using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SortMyStuffAPI.Models;
using SortMyStuffAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using System.Net;

namespace SortMyStuffAPI.Controllers
{
    [Route("/[controller]")]
    [ApiVersion("0.1")]
    public class CategoriesController :
        EntityResourceBaseController<Category, CategoryEntity>
    {
        private readonly ICategoryDataService _categoryDataService;

        public CategoriesController(
            ICategoryDataService categoryDataService,
            IOptions<PagingOptions> pagingOptions,
            IOptions<ApiConfigs> apiConfigs,
            IUserDataService userDataService,
            IHostingEnvironment env,
            IAuthorizationService authService) :
            base(categoryDataService, 
                pagingOptions, 
                apiConfigs, 
                userDataService,
                env,
                authService)
        {
            _categoryDataService = categoryDataService;
        }

        // GET /categories
        [Authorize]
        [HttpGet(Name = nameof(GetCategoriesAsync))]
        public async Task<IActionResult> GetCategoriesAsync(
            CancellationToken ct,
            [FromQuery] PagingOptions pagingOptions,
            [FromQuery] SortOptions<Category, CategoryEntity> sortOptions,
            [FromQuery] SearchOptions<Category, CategoryEntity> searchOptions)
        {
            return await GetResourcesAsync(
                nameof(GetCategoriesAsync),
                ct,
                pagingOptions,
                sortOptions,
                searchOptions);
        }

        // GET /categories/{categoryId}
        [Authorize]
        [HttpGet("{categoryId}", Name = nameof(GetCategoryByIdAsync))]
        public async Task<IActionResult> GetCategoryByIdAsync(string categoryId, CancellationToken ct)
        {
            return await GetResourceByIdAsync(categoryId, ct);
        }

        // POST /categories/
        [Authorize]
        [HttpPost(Name = nameof(CreateCatgegoryAsync))]
        public async Task<IActionResult> CreateCatgegoryAsync(
            [FromBody] CategoryForm body,
            CancellationToken ct)
        {
            var errorMsg = "POST category failed.";
            if (!ModelState.IsValid) return BadRequest(
                new ApiError(errorMsg, ModelState));

            return await CreateResourceAsync(
                nameof(GetCategoryByIdAsync),
                Guid.NewGuid().ToString(),
                body,
                ct,
                errorMessage: errorMsg);
        }

        // PUT /categories/{categoryId}
        [Authorize]
        [HttpPut("{categoryId}", Name = nameof(AddOrUpdateCategoryAsync))]
        public async Task<IActionResult> AddOrUpdateCategoryAsync(
            string categoryId,
            [FromBody] CategoryForm body,
            CancellationToken ct)
        {
            var errorMsg = "PUT category failed.";
            if (!ModelState.IsValid) return BadRequest(new ApiError(ModelState));

            var userId = await GetUserId();

            var record = await DataService.GetResourceAsync(userId, categoryId, ct);

            IActionResult response = null;
            if (record == null)
            {
                response = await CreateResourceAsync(
                    nameof(GetCategoryByIdAsync),
                    categoryId,
                    body,
                    ct,
                    errorMessage: errorMsg);

                if ((response as ObjectResult).StatusCode
                    .Equals(HttpStatusCode.Created))
                {
                    return Ok();
                }

                return response;
            }

            return await UpdateResourceAsync(
                record, body, ct, errorMessage: errorMsg);
        }

        // DELETE /categories/{categoryId}
        [Authorize]
        [HttpDelete("{categoryId}", Name = nameof(DeleteCategoryByIdAsync))]
        public async Task<IActionResult> DeleteCategoryByIdAsync(
            string categoryId,
            [FromQuery] DeletingOptions deletingOptions,
            CancellationToken ct)
        {
            var errorMsg = "Delete category failed.";
            if (!ModelState.IsValid) return BadRequest(
                new ApiError(errorMsg, ModelState));

            var userId = await GetUserId();

            try
            {
                var result = await _categoryDataService.DeleteResourceAsync(
                    userId, categoryId, deletingOptions.DelDependents, ct);

                if (!result.Succeeded)
                {
                    return BadRequest(new ApiError(errorMsg, result.Error));
                }
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        #region PRIVATE METHODS


        #endregion
    }
}
