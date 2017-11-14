using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SortMyStuffAPI.Exceptions;
using SortMyStuffAPI.Models;
using SortMyStuffAPI.Services;
using SortMyStuffAPI.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;

namespace SortMyStuffAPI.Controllers
{
    public abstract class EntityResourceBaseController<T, TEntity> : ApiBaseController
        where T : Resource
        where TEntity : IEntity
    {
        protected readonly IDataService<T, TEntity> DataService;
        protected readonly PagingOptions DefaultPagingOptions;
        private IUserDataService userDataService1;
        private IOptions<PagingOptions> defaultPagingOptions;
        private IOptions<ApiConfigs> apiConfigs;
        private IUserDataService userDataService2;

        protected EntityResourceBaseController(
            IDataService<T, TEntity> dataService,
            IOptions<PagingOptions> defaultPagingOptions,
            IOptions<ApiConfigs> apiConfigs,
            IUserDataService userDataService,
            IHostingEnvironment env,
            IAuthorizationService authService) 
            : base (userDataService,
                  apiConfigs,
                  env,
                  authService)
        {
            DataService = dataService;
            DefaultPagingOptions = defaultPagingOptions.Value;
        }

        protected virtual async Task<IActionResult> GetResourcesAsync(
            string httpMethodName,
            CancellationToken ct,
            [FromQuery] PagingOptions pagingOptions,
            [FromQuery] SortOptions<T, TEntity> sortOptions,
            [FromQuery] SearchOptions<T, TEntity> searchOptions,
            Action<PagedResults<T>> operation = null)
        {
            // if any Model (in this case PagingOptions) property is not valid according to the Range attributes
            if (!ModelState.IsValid) return BadRequest(new ApiError(ModelState));

            pagingOptions.Offset = pagingOptions.Offset ?? DefaultPagingOptions.Offset;
            pagingOptions.PageSize = pagingOptions.PageSize ?? DefaultPagingOptions.PageSize;

            string userId = await GetUserId();

            PagedResults<T> results;
            try
            {
                results = await DataService.GetResouceCollectionAsync(
                    userId,
                    ct, 
                    pagingOptions, 
                    sortOptions, 
                    searchOptions);
            }
            catch (InvalidSearchOperationException ex)
            {
                return BadRequest(new ApiError(ex.Message));
            }

            operation?.Invoke(results);

            var response = PagedCollection<T>.Create(
                Link.ToCollection(httpMethodName),
                results.PagedItems.ToArray(),
                results.TotalSize,
                pagingOptions);

            return Ok(response);
        }

        protected virtual async Task<IActionResult> GetResourceByIdAsync(
            string id,
            CancellationToken ct,
            Action<T> operation = null)
        {
            string userId = await GetUserId();

            var result = await DataService.GetResourceAsync(userId, id, ct);
            if (result == null) return NotFound();

            operation?.Invoke(result);

            return Ok(result);
        }
    }
}
