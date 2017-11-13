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

namespace SortMyStuffAPI.Controllers
{
    [Route("/[controller]")]
    [ApiVersion("0.1")]
    public class CategoriesController :
        JsonResourcesController<Category, CategoryEntity>
    {
        private readonly ICategoryDataService _categoryDataService;

        public CategoriesController(
            ICategoryDataService categoryDataService,
            IOptions<PagingOptions> pagingOptions,
            IOptions<ApiConfigs> apiConfigs) :
            base(categoryDataService, pagingOptions, apiConfigs)
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

            ApiError error;
            if ((error = await CheckNameConflict(body.Name, ct)) != null)
                return BadRequest(error);

            var category = new Category
            {
                Id = Guid.NewGuid().ToString(),
                Name = body.Name
            };

            await _categoryDataService.AddOrUpdateAssetAsync(category, ct);

            var categoryCreated = await _categoryDataService.GetResourceAsync(category.Id, ct);

            return Created(
                Url.Link(nameof(GetCategoryByIdAsync),
                    new { categoryId = category.Id }),
                categoryCreated);
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

            var record = await _categoryDataService.GetResourceAsync(categoryId, ct);
            if (record == null)
            {
                if (!Guid.TryParse(categoryId, out var result))
                    return BadRequest(new ApiError("The assetId must be a correctly formatted Guid."));

                ApiError error;
                if ((error = await CheckNameConflict(body.Name, ct)) != null)
                    return BadRequest(error);

                var category = Mapper.Map<CategoryForm, Category>(body);
                category.Id = categoryId;
                await _categoryDataService.AddOrUpdateAssetAsync(category, ct);
                return Ok();
            }

            record.Name = body.Name;

            await _categoryDataService.AddOrUpdateAssetAsync(record, ct);

            return Ok();
        }

        // DELETE /categories/{categoryId}
        [Authorize]
        [HttpDelete("{categoryId}", Name = nameof(DeleteCategoryByIdAsync))]
        public async Task<IActionResult> DeleteCategoryByIdAsync(
            string categoryId,
            CancellationToken ct)
        {
            // TODO: delete the base details first
            try
            {
                await _categoryDataService.DeleteCategoryAsync(categoryId, ct);
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

        private async Task<ApiError> CheckNameConflict(string newName, CancellationToken ct)
        {
            var conflict = await _categoryDataService.GetCategoryByNameAsync(newName, ct);
            if (conflict != null)
            {
                var msg = "Name conflict with existing category";
                var detail = $"Category id: {conflict.Id}";
                return new ApiError(msg, detail);
            }
            return null;
        }

        #endregion
    }
}
