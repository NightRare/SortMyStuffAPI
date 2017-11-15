using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SortMyStuffAPI.Models;
using SortMyStuffAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using SortMyStuffAPI.Utils;

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
            if (!ModelState.IsValid) return BadRequest(new ApiError(ModelState));

            return await CreateResourceAsync(
                nameof(GetCategoryByIdAsync),
                body,
                ct);
        }

        // PUT /categories/{categoryId}
        [Authorize]
        [HttpPut("{categoryId}", Name = nameof(AddOrUpdateCategoryAsync))]
        public async Task<IActionResult> AddOrUpdateCategoryAsync(
            string categoryId,
            [FromBody] CategoryForm body,
            CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiError(ModelState));

            return await AddOrUpdateResourceAsync(categoryId, body, ct);
        }

        // DELETE /categories/{categoryId}
        [Authorize]
        [HttpDelete("{categoryId}", Name = nameof(DeleteCategoryByIdAsync))]
        public async Task<IActionResult> DeleteCategoryByIdAsync(
            string categoryId,
            CancellationToken ct)
        {
            var userId = await GetUserId();

            // TODO: delete the base details first
            try
            {
                await _categoryDataService.DeleteCategoryAsync(userId, categoryId, ct);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException)
            {
                return BadRequest(
                    new ApiError("Please make sure no asset is assigned to this category before deleting."));
            }
            return NoContent();
        }

        #region PRIVATE METHODS


        #endregion
    }
}
