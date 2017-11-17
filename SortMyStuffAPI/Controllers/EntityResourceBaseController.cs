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
using SortMyStuffAPI.Utils;
using System.Reflection;
using System.Collections.Generic;

namespace SortMyStuffAPI.Controllers
{
    public abstract class EntityResourceBaseController<T, TEntity> : ApiBaseController
        where T : EntityResource
        where TEntity : class, IEntity
    {
        protected readonly IDataService<T, TEntity> DataService;
        protected readonly PagingOptions DefaultPagingOptions;

        protected EntityResourceBaseController(
            IDataService<T, TEntity> dataService,
            IOptions<PagingOptions> defaultPagingOptions,
            IOptions<ApiConfigs> apiConfigs,
            IUserDataService userDataService,
            IHostingEnvironment env,
            IAuthorizationService authService)
            : base(userDataService,
                  apiConfigs,
                  env,
                  authService)
        {
            DataService = dataService;
            DefaultPagingOptions = defaultPagingOptions.Value;
        }

        protected virtual async Task<IActionResult> GetResourcesAsync(
            string responseMethodName,
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

            if (!ServicesAuthHelper.IsDeveloper(userId))
            {
                foreach (var res in results.PagedItems)
                {
                    res.UserId = null;
                }
            }

            operation?.Invoke(results);

            var response = PagedCollection<T>.Create(
                Link.ToCollection(responseMethodName),
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

            if (!ServicesAuthHelper.IsDeveloper(userId))
            {
                result.UserId = null;
            }

            operation?.Invoke(result);

            return Ok(result);
        }

        protected virtual async Task<IActionResult> CreateResourceAsync(
            string responseMethodName,
            string resourceId,
            [FromBody] RequestForm createForm,
            CancellationToken ct,
            Action<T> operation = null,
            string errorMessage = "Create resource failed")
        {
            string userId = await GetUserId();

            // newly added asset via api must have a correctly formatted guid
            if (!Guid.TryParse(resourceId, out var guid))
            {
                return BadRequest(new ApiError(
                    errorMessage,
                    $"The {typeof(T).Name.ToCamelCase()}Id must be a correctly formatted Guid."));
            }

            var resource = Mapper.Map<T>(createForm);
            resource.Id = resourceId;
            resource.UserId = userId;

            var uniqueCheck = await CheckResourceUniqueProperties(
                userId, resource, createForm, ct);
            if (!uniqueCheck.IsUnique)
                return uniqueCheck.ErrorResult;

            operation?.Invoke(resource);

            var result = await DataService.AddResourceAsync(userId, resource, ct);
            if (!result.Succeeded)
            {
                return BadRequest(new ApiError(
                    errorMessage, result.Error));
            }

            var assetCreated = await DataService.GetResourceAsync(
                userId,
                resource.Id,
                ct);

            return Created(
                Url.Link(responseMethodName,
                GetLinkValue(resource.GetType(), resource.Id)),
                assetCreated);
        }

        protected async Task<IActionResult> UpdateResourceAsync(
            T record,
            [FromBody] RequestForm updateForm,
            CancellationToken ct,
            Action<T> operation = null,
            string errorMessage = "PATCH operation failed")
        {
            var userId = await GetUserId();

            (bool IsUnique, IActionResult ErrorResult) uniqueCheck;

            var immutableCheck = CheckImmutableProperties(updateForm, record);
            if (!immutableCheck.Pass)
            {
                return BadRequest(new ApiError(errorMessage, immutableCheck.Error));
            }

            var update = Mapper.Map<T>(updateForm);
            update.Id = record.Id;
            update.UserId = record.UserId;

            uniqueCheck = await CheckResourceUniqueProperties(
                record.UserId, update, updateForm, ct);
            if (!uniqueCheck.IsUnique)
            {
                return uniqueCheck.ErrorResult;
            }

            operation?.Invoke(update);

            var updateResult = await DataService.UpdateResourceAsync(userId, update, ct);
            if (!updateResult.Succeeded)
            {
                return BadRequest(new ApiError(
                    $"Update {typeof(T).Name.ToCamelCase()} failed.",
                    updateResult.Error));
            }

            return Ok();
        }

        #region PRIVATE METHODS

        private static object GetLinkValue(Type typeOfT, string guid)
        {
            if (typeOfT == typeof(Asset))
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

            if(typeOfT == typeof(BaseDetail))
            {
                return new { baseDetailId = guid }; 
            }

            return null;
        }

        private async Task<(bool IsUnique, IActionResult ErrorResult)>
            CheckResourceUniqueProperties(
                string userId,
                T resource,
                RequestForm form,
                CancellationToken ct)
        {
            var scopedUniqueProperties = form.GetType()
                .GetProperties()
                .Where(p => p.GetCustomAttributes<ScopedUniqueAttribute>().Any());

            var errorMessages = new List<string>();
            foreach (var prop in scopedUniqueProperties)
            {
                var property = resource.GetType().GetProperty(prop.Name) ??
                    throw new ApiException(
                        $"Invalid form property [{prop.Name}] in type {form.GetType().Name}.");

                var scope = prop.GetCustomAttribute<ScopedUniqueAttribute>().Scope;
                if (!await DataService.CheckScopedUniquenessAsync(
                    userId, property, resource, scope, ct))
                {
                    errorMessages.Add($"Duplicate [Property] {prop.Name.ToCamelCase()}; " +
                        $"[Value] {property.GetValue(resource)}" + Environment.NewLine);
                }
            }

            if (errorMessages.Any())
            {
                var errorMessage = String.Concat(errorMessages);
                return (false, BadRequest(new ApiError(
                    "Conflict values found.", errorMessage)));
            }
            return (true, null);
        }

        private (bool Pass, string Error) CheckImmutableProperties(RequestForm form, T existingResource)
        {
            var allImmutableProperties = form.GetType()
                .GetProperties()
                .Where(p => p.GetCustomAttributes<ImmutableAttribute>().Any());

            foreach (var prop in allImmutableProperties)
            {
                var value = prop.GetValue(form);
                bool changed = !value.Equals(
                    existingResource.GetType()
                        .GetProperty(prop.Name)?
                        .GetValue(existingResource));
                if (changed)
                {
                    return (false, $"Update {typeof(T).Name.ToCamelCase()} failed." +
                        $"{prop.Name} cannot be modified.");
                }
            }
            return (true, null);
        }

        #endregion
    }
}
