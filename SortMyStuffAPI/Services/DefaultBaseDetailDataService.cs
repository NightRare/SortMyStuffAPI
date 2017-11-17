using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SortMyStuffAPI.Infrastructure;
using SortMyStuffAPI.Models;

namespace SortMyStuffAPI.Services
{
    public class DefaultBaseDetailDataService
        : DefaultBaseDataService, 
        IBaseDetailDataService
    {

        private readonly IDetailDataService _detailDataService;

        public DefaultBaseDetailDataService(
            SortMyStuffContext dbContext,
            IOptions<ApiConfigs> apiConfigs,
            IDetailDataService detailDataService)
            : base(dbContext, apiConfigs)
        {
            _detailDataService = detailDataService;
        }

        public async Task<BaseDetail> GetResourceAsync(
            string userId,
            string resourceId,
            CancellationToken ct)
        {
            return await GetOneResourceAsync<BaseDetail, BaseDetailEntity>(
                DbContext.BaseDetails,
                userId,
                resourceId,
                ct);
        }

        public async Task<PagedResults<BaseDetail>> GetResouceCollectionAsync(
            string userId,
            CancellationToken ct,
            PagingOptions pagingOptions = null,
            SortOptions<BaseDetail, BaseDetailEntity> sortOptions = null,
            SearchOptions<BaseDetail, BaseDetailEntity> searchOptions = null)
        {
            return await GetOneTypeResourcesAsync(
                userId,
                DbContext.BaseDetails,
                ct,
                pagingOptions,
                sortOptions,
                searchOptions);
        }

        public Task<(bool Succeeded, string Error)> AddResourceAsync(
            string userId,
            BaseDetail resource,
            CancellationToken ct)
        {
            return AddOneResourceAsync(
                userId,
                resource,
                DbContext.BaseDetails,
                ct);
        }

        public async Task<(bool Succeeded, string Error)> UpdateResourceAsync(
            string userId,
            BaseDetail resource,
            CancellationToken ct)
        {
            return await UpdateOneResourceAsync(
                userId,
                resource,
                DbContext.BaseDetails,
                ct);
        }

        public async Task<(bool Succeeded, string Error)> DeleteResourceAsync(
            string userId,
            string resourceId,
            bool delDependents,
            CancellationToken ct)
        {
            var repo = GetUserRepository(userId, DbContext.BaseDetails);

            var entity = repo.SingleOrDefault(b => b.Id == resourceId) ??
                throw new KeyNotFoundException();

            if (!delDependents)
            {
                if (entity.Derivatives.Any())
                {
                    return (false, "Derivative details found. " +
                        "Cannot delete a base detail with dependents.");
                }
                return await DeleteOneResourceAsync(resourceId, repo, ct);
            }

            return await DeleteBaseDetailWithDependentsAsync(
                entity, repo, ct);
        }

        public async Task<bool> CheckScopedUniquenessAsync(
            string userId,
            PropertyInfo property,
            BaseDetail resource,
            Scope scope,
            CancellationToken ct)
        {
            return await Task.Run(() =>
                CheckResourceScopedUniqueness(
                    userId,
                    property,
                    resource,
                    scope,
                    DbContext.BaseDetails), ct);
        }

        #region PRIVATE METHODS

        private async Task<(bool Succeeded, string Error)>
            DeleteBaseDetailWithDependentsAsync(
                BaseDetailEntity entity,
                IQueryable<BaseDetailEntity> repo,
                CancellationToken ct)
        {
            var derivativeDetails = entity.Derivatives.ToArray();

            foreach (var detail in derivativeDetails)
            {
                var delDetail = await _detailDataService
                    .DeleteResourceAsync(entity.UserId, detail.Id, true, ct);

                if (!delDetail.Succeeded)
                {
                    return (false, delDetail.Error);
                }
            }

            return await DeleteOneResourceAsync(entity.Id, repo, ct);
        }

        #endregion
    }
}
