using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SortMyStuffAPI.Infrastructure;
using SortMyStuffAPI.Models;

namespace SortMyStuffAPI.Services
{
    public class DefaultDetailDataService
        : DefaultDataService<Detail, DetailEntity>,
        IDetailDataService
    {
        public DefaultDetailDataService(
            SortMyStuffContext dbContext,
            IOptions<ApiConfigs> apiConfigs) :
            base(dbContext, apiConfigs)
        {
        }

        #region IDataService METHODS

        public async Task<Detail> GetResourceAsync(
            string userId,
            string resourceId,
            CancellationToken ct)
        {
            return await GetOneResourceAsync(
                DbContext.Details,
                userId,
                resourceId,
                ct);
        }

        public async Task<PagedResults<Detail>> GetResouceCollectionAsync(
            string userId,
            CancellationToken ct,
            PagingOptions pagingOptions = null,
            SortOptions<Detail, DetailEntity> sortOptions = null,
            SearchOptions<Detail, DetailEntity> searchOptions = null)
        {
            return await GetOneTypeResourcesAsync(
                userId,
                DbContext.Details,
                ct,
                pagingOptions,
                sortOptions,
                searchOptions);
        }

        public async Task<(bool Succeeded, string Error)> AddResourceAsync(
            string userId,
            Detail resource,
            CancellationToken ct)
        {
            return await AddOneResourceAsync(
                userId,
                resource,
                DbContext.Details,
                ct);
        }

        public async Task<(bool Succeeded, string Error)> AddResourceCollectionAsync(
            string userId,
            ICollection<Detail> resources,
            CancellationToken ct)
        {
            return await AddResourceRangeAsync(
                userId,
                resources,
                DbContext.Details,
                ct);
        }

        public async Task<(bool Succeeded, string Error)> UpdateResourceAsync(
            string userId,
            Detail resource,
            CancellationToken ct)
        {
            return await UpdateOneResourceAsync(
                userId,
                resource,
                DbContext.Details,
                ct);
        }

        public async Task<(bool Succeeded, string Error)> DeleteResourceAsync(
            string userId,
            string resourceId,
            bool delDependents,
            CancellationToken ct)
        {
            var repo = GetUserRepository(userId, DbContext.Details);

            // details have no dependents
            return await DeleteOneResourceAsync(resourceId, repo, ct);
        }

        public async Task<bool> CheckScopedUniquenessAsync(
            string userId,
            PropertyInfo property,
            Detail resource,
            Scope scope,
            CancellationToken ct)
        {
            return await Task.Run(() =>
                CheckResourceScopedUniqueness(
                    userId,
                    property,
                    resource,
                    scope,
                    DbContext.Details), ct);
        }

        #endregion
    }
}
