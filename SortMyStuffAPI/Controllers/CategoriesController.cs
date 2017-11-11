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
    public class CategoriesController : JsonResourcesController<Category, CategoryEntity>
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
        [HttpGet("{categoryId}", Name = nameof(GetCategoryByIdAsync))]
        public async Task<IActionResult> GetCategoryByIdAsync(string categoryId, CancellationToken ct)
        {
            return await GetResourceByIdAsync(categoryId, ct);
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
