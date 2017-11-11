using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SortMyStuffAPI.Exceptions;
using SortMyStuffAPI.Models;
using SortMyStuffAPI.Services;

namespace SortMyStuffAPI.Controllers
{
    public abstract class JsonResourcesController<T, TEntity> : Controller 
        where T : Resource
        where TEntity : IEntity
    {
        protected readonly IDataService<T, TEntity> DataService;
        protected readonly PagingOptions DefaultPagingOptions;
        protected readonly ApiConfigs ApiConfigs;

        protected JsonResourcesController(
            IDataService<T, TEntity> dataService, 
            IOptions<PagingOptions> defaultPagingOptions,
           IOptions<ApiConfigs> apiConfigs)
        {
            DataService = dataService;
            DefaultPagingOptions = defaultPagingOptions.Value;
            ApiConfigs = apiConfigs.Value;
        }

        protected virtual async Task<IActionResult> GetResourcesAsync(
            string httpMethodName,
            CancellationToken ct,
            [FromQuery] PagingOptions pagingOptions,
            [FromQuery] SortOptions<T, TEntity> sortOptions,
            [FromQuery] SearchOptions<T, TEntity> searchOptions
        )
        {
            // if any Model (in this case PagingOptions) property is not valid according to the Range attributes
            if (!ModelState.IsValid) return BadRequest(new ApiError(ModelState));

            pagingOptions.Offset = pagingOptions.Offset ?? DefaultPagingOptions.Offset;
            pagingOptions.PageSize = pagingOptions.PageSize ?? DefaultPagingOptions.PageSize;

            PagedResults<T> results;
            try
            {
                results = await DataService.GetResouceCollectionAsync(
                    ct, pagingOptions, sortOptions, searchOptions);
            }
            catch (InvalidSearchOperationException ex)
            {
                return BadRequest(new ApiError(ex.Message));
            }

            var response = PagedCollection<T>.Create(
                Link.ToCollection(httpMethodName),
                results.PagedItems.ToArray(),
                results.TotalSize,
                pagingOptions);

            return Ok(response);
        }

        protected virtual async Task<IActionResult> GetResourceByIdAsync(
            string id,
            CancellationToken ct)
        {
            var asset = await DataService.GetResourceAsync(id, ct);
            if (asset == null) return NotFound();

            return Ok(asset);
        }
    }
}
