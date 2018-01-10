using SortMyStuffAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Threading;
using SortMyStuffAPI.Utils;
using System.Reflection;
using SortMyStuffAPI.Infrastructure;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace SortMyStuffAPI.Services
{
    public class DefaultAssetDataService
        : DefaultDataService<Asset, AssetEntity>,
        IAssetDataService
    {
        private readonly IUserService _userService;
        private readonly IDetailDataService _detailDataService;

        public DefaultAssetDataService(
            SortMyStuffContext dbContext,
            IOptions<ApiConfigs> apiConfigs,
            IUserService userService,
            IDetailDataService detailDataService)
            : base(dbContext, apiConfigs)
        {
            _userService = userService;
            _detailDataService = detailDataService;
        }

        #region IDataService METHODS

        public async Task<Asset> GetResourceAsync(
            string userId,
            string id,
            CancellationToken ct)
        {
            return await GetOneResourceAsync(
                DbContext.Assets,
                userId,
                id,
                ct);
        }

        public async Task<PagedResults<Asset>> GetResouceCollectionAsync(
            string userId,
            CancellationToken ct,
            PagingOptions pagingOptions = null,
            SortOptions<Asset, AssetEntity> sortOptions = null,
            SearchOptions<Asset, AssetEntity> searchOptions = null)
        {
            return await GetOneTypeResourcesAsync(
                userId,
                DbContext.Assets,
                ct,
                pagingOptions,
                sortOptions,
                searchOptions);
        }

        public async Task<(bool Succeeded, string Error)> AddResourceAsync(
            string userId,
            Asset resource,
            CancellationToken ct)
        {
            return await AddOneResourceAsync(
                userId,
                resource,
                DbContext.Assets,
                ct);
        }

        public async Task<(bool Succeeded, string Error)> AddResourceCollectionAsync(
            string userId,
            ICollection<Asset> resources,
            CancellationToken ct)
        {
            return await AddResourceRangeAsync(
                userId,
                resources,
                DbContext.Assets,
                ct);
        }


        public async Task<(bool Succeeded, string Error)> UpdateResourceAsync(
            string userId,
            Asset resource,
            CancellationToken ct)
        {
            return await UpdateOneResourceAsync(
                userId,
                resource,
                DbContext.Assets,
                ct);
        }

        public async Task<(bool Succeeded, string Error)> DeleteResourceAsync(
            string userId,
            string resourceId,
            bool delDependents)
        {
            var assetRepo = GetUserRepository(userId, DbContext.Assets);

            if (!delDependents)
            {
                if (assetRepo.Any(a => a.ContainerId == resourceId))
                {
                    return (false, "Content assets found. Cannot delete an asset with dependents.");
                }
                return await DeleteOneResourceAsync(resourceId, assetRepo);
            }

            return await DeleteAssetWithDependentsAsync(
                userId, resourceId, assetRepo, GetUserRepository(userId, DbContext.Details));
        }

        public async Task<bool> CheckScopedUniquenessAsync(
            string userId,
            PropertyInfo property,
            Asset resource,
            Scope scope,
            CancellationToken ct)
        {
            return await Task.Run(() =>
                CheckResourceScopedUniqueness(
                    userId,
                    property,
                    resource,
                    scope,
                    DbContext.Assets), ct);
        }

        #endregion


        #region IAssetDataService METHODS

        public async Task<AssetTree> GetAssetTreeAsync(
            string userId,
            string id,
            CancellationToken ct)
        {
            var repo = GetUserRepository(userId, DbContext.Assets);
            var tree = await Task.Run(() =>
            {
                var entity = repo.SingleOrDefault(a => a.Id == id);
                return entity == null ? null : ConvertToAssetTree(entity);
            }, ct);

            return tree;
        }

        public async Task<IEnumerable<PathUnit>> GetAssetPathAsync(
            string userId,
            string id,
            CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                var parents = GetAllParents(userId, id);
                return GetAllParents(userId, id)
                    .Select(entity =>
                        new PathUnit
                        {
                            Name = entity.Name,
                            Asset = Link.To(nameof(
                                    Controllers.AssetsController.GetAssetByIdAsync),
                                    new { assetId = entity.Id })
                        });
            }, ct);
        }

        public async Task<IEnumerable<Asset>> GetAssetsByCategoryId(
            string userId,
            string categoryId,
            CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                return GetUserRepository(userId, DbContext.Assets)
                    // if a.CategoryId == null then it's root asset
                    .Where(a => a.CategoryId != null && a.CategoryId == categoryId)
                    .ProjectTo<Asset>()
                    .ToArray();
            }, ct);
        }

        public async Task<(bool Succeeded, string Error)> CreateRootAssetForUserAsync(
            string userId, CancellationToken ct)
        {
            var user = await _userService.GetUserByIdAsync(userId) ??
                throw new KeyNotFoundException($"The user {userId} does not exist.");

            var currentTime = DateTimeOffset.UtcNow;
            var root = new AssetEntity
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Name = ApiStrings.RootAssetDefaultName,
                CategoryId = null,
                ContainerId = ApiStrings.RootAssetToken,
                CreateTimestamp = currentTime,
                ModifyTimestamp = currentTime
            };

            user.RootAssetId = root.Id;

            try
            {
                DbContext.Assets.Add(root);

                await _userService.UpdateAsync(user);
                DbContext.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                return (false, ex.Message);
            }

            return (true, null);
        }

        #endregion


        #region PRIVATE METHODS

        private IList<AssetEntity> GetAllParents(
            string userId,
            string id)
        {
            var repo = GetUserRepository(userId, DbContext.Assets);
            var entity = repo.SingleOrDefault(a => a.Id == id) ??
                throw new KeyNotFoundException();

            var parents = new List<AssetEntity>();
            parents.Add(entity);
            while (entity.ContainerId != null && !entity.ContainerId.Equals(ApiStrings.RootAssetToken))
            {
                entity = repo.SingleOrDefault(a => a.Id == entity.ContainerId);
                if (entity == null) break;
                parents.Add(entity);
            }
            return parents;
        }

        private AssetTree ConvertToAssetTree(AssetEntity assetEntity)
        {
            if (assetEntity == null) return null;
            var contents = DbContext.Assets.Where(a => a.ContainerId == assetEntity.Id);

            var assetTree = new AssetTree
            {
                Self = Link.To(nameof(Controllers.AssetTreesController.GetAssetTreeByIdAsync),
                    new { assetId = assetEntity.Id }),
                Id = assetEntity.Id,
                Name = assetEntity.Name,
                Contents = null
            };

            var childAssetTrees = new List<AssetTree>();
            foreach (var child in contents)
            {
                childAssetTrees.Add(ConvertToAssetTree(child));
            }
            assetTree.Contents = childAssetTrees.ToArray();

            return assetTree;
        }

        private void DeleteAsset(AssetEntity entity)
        {
            var contents = DbContext.Assets.Where(a => a.ContainerId == entity.Id);
            foreach (var asset in contents)
            {
                DeleteAsset(asset);
            }
            DbContext.Assets.Remove(entity);
        }

        private async Task<(bool Succeeded, string Error)> DeleteAssetWithDependentsAsync(
            string userId,
            string resourceId,
            IQueryable<AssetEntity> assetRepo,
            IQueryable<DetailEntity> detailRepo)
        {
            var contents = assetRepo.Where(a => a.ContainerId == (resourceId));
            if (contents.Any())
            {
                foreach (var asset in contents)
                {
                    var result = await DeleteAssetWithDependentsAsync(userId, asset.Id, assetRepo, detailRepo);
                    if (!result.Succeeded)
                    {
                        return result;
                    }
                }
            }

            var details = detailRepo.Where(d => d.AssetId == (resourceId));
            foreach (var detail in details)
            {
                await _detailDataService.DeleteResourceAsync(userId, detail.Id, true);
            }
            return await DeleteOneResourceAsync(resourceId, assetRepo);
        }

        #endregion
    }
}
