using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SortMyStuffAPI.Models;
using SortMyStuffAPI.Services;

namespace SortMyStuffAPI.Controllers
{
    [Route("/[controller]")]
    [ApiVersion("0.1")]
    public class CategoriesController : Controller
    {
        private readonly ICategoryDataService _categoryDataService;
        private readonly PagingOptions _defaultPagingOptions;
        private readonly ApiConfigs _apiConfigs;

        public CategoriesController(
            ICategoryDataService categoryDataService,
            IOptions<PagingOptions> pagingOptions,
            IOptions<ApiConfigs> apiConfigs)
        {
            _categoryDataService = categoryDataService;
            _defaultPagingOptions = pagingOptions.Value;
            _apiConfigs = apiConfigs.Value;
        }

        // GET /categories
        [HttpGet(Name = nameof(GetCategoriesAsync))]
        public async Task<IActionResult> GetCategoriesAsync(
            CancellationToken ct,
            [FromQuery] PagingOptions pagingOptions,
            [FromQuery] SortOptions<Category, CategoryEntity> sortOptions,
            [FromQuery] SearchOptions<Category, CategoryEntity> searchOptions)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiError(ModelState));

            pagingOptions.Offset = pagingOptions.Offset ?? _defaultPagingOptions.Offset;
            pagingOptions.PageSize = pagingOptions.PageSize ?? _defaultPagingOptions.PageSize;

            var categories = await _categoryDataService
                .GetAllCategoriesAsync(ct, pagingOptions, sortOptions, searchOptions);

            var response = PagedCollection<Category>.Create(
                Link.ToCollection(nameof(GetCategoriesAsync)),
                categories.PagedItems.ToArray(),
                categories.TotalSize,
                pagingOptions);

            return Ok(response);
        }

        // GET /categories/{categoryId}
        [HttpGet("{categoryId}", Name = nameof(GetCategoryByIdAsync))]
        public async Task<IActionResult> GetCategoryByIdAsync(string categoryId, CancellationToken ct)
        {
            var category = await _categoryDataService.GetCategoryAsync(categoryId, ct);
            if (category == null) return NotFound();

            return Ok(category);
        }

        // PUT /categories/{categoryId}
        [HttpPut("{categoryId}", Name = nameof(AddOrUpdateCategoryAsync))]
        public async Task<IActionResult> AddOrUpdateCategoryAsync(
            string categoryId,
            CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        // POST /categories/
        [HttpPost(Name = nameof(CreateCatgegoryAsync))]
        public async Task<IActionResult> CreateCatgegoryAsync(
            CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        [HttpDelete("{categoryId}", Name = nameof(DeleteCategoryByIdAsync))]
        public async Task<IActionResult> DeleteCategoryByIdAsync(
            string categoryId,
            CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
