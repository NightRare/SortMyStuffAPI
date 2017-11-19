using SortMyStuffAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Reflection;
using SortMyStuffAPI.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace SortMyStuffAPI.Services
{
    public class DefaultCategoryDataService
        : DefaultDataService<Category, CategoryEntity>,
        ICategoryDataService
    {
        private readonly IAssetDataService _assetDataService;
        private readonly IBaseDetailDataService _baseDetailDataService;

        public DefaultCategoryDataService(
            SortMyStuffContext dbContext,
            IOptions<ApiConfigs> apiConfigs,
            IAssetDataService assetDataService,
            IBaseDetailDataService baseDetailDataService)
            : base(dbContext, apiConfigs)
        {
            _assetDataService = assetDataService;
            _baseDetailDataService = baseDetailDataService;
        }

        #region IDataService METHODS

        public async Task<Category> GetResourceAsync(
            string userId,
            string id,
            CancellationToken ct)
        {
            return await GetOneResourceAsync(
                DbContext.Categories,
                userId,
                id,
                ct);
        }

        public async Task<PagedResults<Category>> GetResouceCollectionAsync(
            string userId,
            CancellationToken ct,
            PagingOptions pagingOptions = null,
            SortOptions<Category, CategoryEntity> sortOptions = null,
            SearchOptions<Category, CategoryEntity> searchOptions = null)
        {
            return await GetOneTypeResourcesAsync(
                userId,
                DbContext.Categories,
                ct,
                pagingOptions,
                sortOptions,
                searchOptions);
        }

        public async Task<(bool Succeeded, string Error)> AddResourceAsync(
            string userId,
            Category resource,
            CancellationToken ct)
        {
            return await AddOneResourceAsync(
                userId,
                resource,
                DbContext.Categories,
                ct);
        }

        public async Task<(bool Succeeded, string Error)> AddResourceCollectionAsync(
            string userId, 
            ICollection<Category> resources, 
            CancellationToken ct)
        {
            return await AddResourceRangeAsync(
                userId,
                resources,
                DbContext.Categories,
                ct);
        }

        public async Task<(bool Succeeded, string Error)> UpdateResourceAsync(
            string userId, Category resource, CancellationToken ct)
        {
            return await UpdateOneResourceAsync(
                userId,
                resource,
                DbContext.Categories,
                ct);
        }

        public async Task<(bool Succeeded, string Error)> DeleteResourceAsync(
            string userId,
            string resourceId,
            bool delDependents,
            CancellationToken ct)
        {
            var repo = GetUserRepository(userId, DbContext.Categories);

            // EF core is not supporting lazy loading yet, need to use eager loading
            var category = repo
                .Include(c => c.CategorisedAssets)
                .Include(c => c.BaseDetails)
                .SingleOrDefault(c => c.Id == resourceId) ??
                throw new KeyNotFoundException();

            if (!delDependents)
            {
                if (category.CategorisedAssets.Any() || category.BaseDetails.Any())
                {
                    return (false, "Categorised assets or base details found. Cannot delete category with dependents.");
                }
                return await DeleteOneResourceAsync(resourceId, repo, ct);
            }

            return await DeleteCategoryWithDependentsAsync(category, repo, ct);
        }

        public async Task<bool> CheckScopedUniquenessAsync(
            string userId,
            PropertyInfo property,
            Category resource,
            Scope scope,
            CancellationToken ct)
        {
            return await Task.Run(() =>
                CheckResourceScopedUniqueness(
                    userId,
                    property,
                    resource,
                    scope,
                    DbContext.Categories), ct);
        }

        #endregion


        #region PRIVATE METHODS

        private async Task<(bool Succeeded, string Error)> DeleteCategoryWithDependentsAsync(
            CategoryEntity category,
            IQueryable<CategoryEntity> repo,
            CancellationToken ct)
        {
            // note that when deleting the navigation property (categorised asset)
            // category.CategorisedAssets will be modifed as well
            // therefore have to iterate through another clone collection
            var categorisedAssets = category.CategorisedAssets.ToArray();
            foreach (var asset in categorisedAssets)
            {
                var delAsset = await _assetDataService
                    .DeleteResourceAsync(category.UserId, asset.Id, true, ct);

                if (!delAsset.Succeeded)
                {
                    return (false, delAsset.Error);
                }
            }

            var baseDetails = category.BaseDetails.ToArray();
            foreach (var bd in baseDetails)
            {
                var delBd = await _baseDetailDataService
                    .DeleteResourceAsync(category.UserId, bd.Id, true, ct);

                if(!delBd.Succeeded)
                {
                    return (false, delBd.Error);
                }
            }

            return await DeleteOneResourceAsync(category.Id, repo, ct);
        }

        #endregion

    }
}
