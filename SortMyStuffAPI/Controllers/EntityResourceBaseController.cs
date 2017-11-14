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
using AutoMapper;
using System.Reflection;
using System.Collections.Generic;

namespace SortMyStuffAPI.Controllers
{
    public abstract class EntityResourceBaseController<T, TEntity> : ApiBaseController
        where T : Resource
        where TEntity : class, IEntity
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

        protected virtual async Task<IActionResult> CreateResourceAsync(
            string httpMethodName,
            [FromBody] FormModel createForm,
            CancellationToken ct,
            Action<T> operation = null)
        {
            var guid = Guid.NewGuid().ToString();
            string userId = await GetUserId();

            var resource = Mapper.Map<T>(createForm);

            // Set the Id value of the resource, if applicable
            var IdProperty = resource
                .GetType().GetProperties()
                .SingleOrDefault(p => p.Name.Equals("Id"));

            IdProperty?.SetValue(resource, guid);

            operation?.Invoke(resource);

            var result = await DataService.AddResourceAsync(userId, resource, ct);
            if (!result.Succeeded)
            {
                return BadRequest(new ApiError(
                    $"Create {resource.GetType().Name.ToCamelCase()} failed.", 
                    result.Error));
            }

            var assetCreated = await DataService.GetResourceAsync(userId, guid, ct);

            return Created(
                Url.Link(httpMethodName,
                GetLinkValue(resource.GetType(), guid)),
                assetCreated);
        }

        private static object GetLinkValue(Type typeOfT, string guid)
        {
            if(typeOfT == typeof(Asset))
            {
                return new { assetId = guid };
            }

            if (typeOfT == typeof(Category))
            {
                return new { categoryId = guid };
            }

            if (typeOfT == typeof(Detail))
            {
                return new { detailId = guid };
            }

            return null;
        }
    }
}
