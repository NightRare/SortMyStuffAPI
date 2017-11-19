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
using System.Net;

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
            IUserService userService,
            IHostingEnvironment env,
            IAuthorizationService authService)
            : base(userService,
                  apiConfigs,
                  env,
                  authService)
        {
            DataService = dataService;
            DefaultPagingOptions = defaultPagingOptions.Value;
        }

        protected virtual async Task<IStatusCodeResult<PagedCollection<T>>> GetResourcesAsync(
            string responseMethodName,
            CancellationToken ct,
            PagingOptions pagingOptions,
            SortOptions<T, TEntity> sortOptions,
            SearchOptions<T, TEntity> searchOptions)
        {
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
                return ResultFactory.FailureCode<PagedCollection<T>>(
                    errorObject: ex,
                    errorMessage: ex.Message);
            }

            if (!ServicesAuthHelper.IsDeveloper(userId))
            {
                HideUserId(results.PagedItems.ToArray());
            }

            var response = PagedCollection<T>.Create(
                Link.ToCollection(responseMethodName),
                results.PagedItems.ToArray(),
                results.TotalSize,
                pagingOptions);

            return ResultFactory.SuccessCode(
                resultObject: response);
        }

        protected virtual async Task<IStatusCodeResult<T>> GetResourceByIdAsync(
            string id,
            CancellationToken ct)
        {
            string userId = await GetUserId();

            var result = await DataService.GetResourceAsync(userId, id, ct);
            if (result == null)
            {
                return ResultFactory.FailureCode<T>(
                    statusCode: HttpStatusCode.NotFound,
                    errorMessage: $"{typeof(T).Name} {id} does not exist.");
            }

            if (!ServicesAuthHelper.IsDeveloper(userId))
            {
                HideUserId(result);
            }

            return ResultFactory.SuccessCode(
                resultObject: result);
        }

        protected virtual async Task<IStatusCodeResult<T>> CreateResourceAsync(
            string resourceId,
            RequestForm createForm,
            CancellationToken ct,
            Action<T> operation = null)
        {
            string userId = await GetUserId();

            // newly added asset via api must have a correctly formatted guid
            if (!Guid.TryParse(resourceId, out var guid))
            {
                return ResultFactory.FailureCode<T>(
                    errorMessage: $"The {typeof(T).Name.ToCamelCase()}Id " +
                    $"must be a correctly formatted Guid.");
            }

            var resource = Mapper.Map<T>(createForm);
            resource.Id = resourceId;
            resource.UserId = userId;

            var checkUnique = await CheckResourceUniqueProperties(
                userId, resource, createForm, ct);
            if (!checkUnique.Succeeded)
            {
                return checkUnique.AddFailureStatusCode<T>();
            }

            operation?.Invoke(resource);

            var result = await DataService.AddResourceAsync(userId, resource, ct);
            if (!result.Succeeded)
            {
                return ResultFactory.FailureCode<T>(
                    errorMessage: result.Error);
            }

            var assetCreated = await DataService.GetResourceAsync(
                userId,
                resource.Id,
                ct);

            if (!ServicesAuthHelper.IsDeveloper(userId))
            {
                HideUserId(assetCreated);
            }

            return ResultFactory.SuccessCode(
                HttpStatusCode.Created, assetCreated);
        }

        protected async Task<IStatusCodeResult> UpdateResourceAsync(
            T record,
            RequestForm updateForm,
            CancellationToken ct,
            Action<T> operation = null)
        {
            var userId = await GetUserId();

            IResult checkUnique;
            var immutableCheck = CheckImmutableProperties(updateForm, record);
            if (!immutableCheck.Pass)
            {
                return ResultFactory.FailureCode(
                    errorMessage: immutableCheck.Error);
            }

            var update = Mapper.Map<T>(updateForm);
            update.Id = record.Id;
            update.UserId = record.UserId;

            checkUnique = await CheckResourceUniqueProperties(
                record.UserId, update, updateForm, ct);
            if (!checkUnique.Succeeded)
            {
                return checkUnique.AddFailureStatusCode();
            }

            operation?.Invoke(update);

            var updateResult = await DataService.UpdateResourceAsync(userId, update, ct);
            if (!updateResult.Succeeded)
            {
                return ResultFactory.FailureCode(
                    errorMessage: updateResult.Error);
            }

            return ResultFactory.SuccessCode();
        }

        protected void HideUserId(
            params EntityResource[] resources)
        {
            foreach (var r in resources)
            {
                r.UserId = null;
            }
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

            if (typeOfT == typeof(BaseDetail))
            {
                return new { baseDetailId = guid };
            }

            return null;
        }

        private async Task<IResult>
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
                return ResultFactory.Failure(
                    errorMessage: String.Concat(errorMessages));
            }
            return ResultFactory.Success();
        }

        private (bool Pass, string Error) CheckImmutableProperties(
            RequestForm form, T existingResource)
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
