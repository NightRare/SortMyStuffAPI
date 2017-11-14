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
using SortMyStuffAPI.Infrastructure;

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

            string userId = await GetUserId();

            ApiError error;
            if ((error = await CheckNameConflict(userId, body.Name, ct)) != null)
                return BadRequest(error);

            var category = new Category
            {
                Id = Guid.NewGuid().ToString(),
                Name = body.Name
            };

            var result = await _categoryDataService.AddCategoryAsync(userId, category, ct);
            if (!result.Succeeded)
            {
                return BadRequest(new ApiError("Create category failed.", result.Error));
            }

            var categoryCreated = await _categoryDataService.GetResourceAsync(userId, category.Id, ct);

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

            string userId = await GetUserId();

            var record = await _categoryDataService.GetResourceAsync(userId, categoryId, ct);

            // [Start Create Category]
            if (record == null)
            {
                userId = await GetUserId();

                if (!Guid.TryParse(categoryId, out var guid))
                    return BadRequest(new ApiError("The categoryId must be a correctly formatted Guid."));

                ApiError error;
                if ((error = await CheckNameConflict(userId, body.Name, ct)) != null)
                    return BadRequest(error);

                var category = Mapper.Map<CategoryForm, Category>(body);
                category.Id = categoryId;
                var createResult = await _categoryDataService.AddCategoryAsync(userId, category, ct);
                if (!createResult.Succeeded)
                {
                    return BadRequest(new ApiError("Create category failed.", createResult.Error));
                }

                return Ok();
            }
            // [End Create Category]

            record.Name = body.Name;

            var updateResult = await _categoryDataService.UpdateCategoryAsync(userId, record, ct);
            if (!updateResult.Succeeded)
            {
                return BadRequest(new ApiError("Update category failed.", updateResult.Error));
            }

            return Ok();
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

        private async Task<ApiError> CheckNameConflict(
            string userId, 
            string newName, 
            CancellationToken ct)
        {
            var conflict = await _categoryDataService.GetCategoryByNameAsync(userId, newName, ct);
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
