using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using SortMyStuffAPI.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SortMyStuffAPI.Infrastructure;
using System.Security.Claims;
using SortMyStuffAPI.Utils;
using System.Reflection;

namespace SortMyStuffAPI.Services
{
    public class DefaultDataService : 
        DefaultBaseDataService,
        IAssetDataService,
        IDetailDataService,
        ICategoryDataService,
        IUserDataService
    {
        private readonly UserManager<UserEntity> _userManager;
        private readonly RoleManager<UserRoleEntity> _roleManager;

        public DefaultDataService(
            SortMyStuffContext context,
            IOptions<ApiConfigs> apiConfigs,
            UserManager<UserEntity> userManager,
            RoleManager<UserRoleEntity> roleManager)
            : base(context, apiConfigs)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }


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

        async Task<Asset> IDataService<Asset, AssetEntity>.GetResourceAsync(
            string userId,
            string id,
            CancellationToken ct)
        {
            return await GetOneResourceAsync<Asset, AssetEntity>(
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

        public async Task<IEnumerable<PathUnit>> GetAssetPathAsync(
            string userId,
            string id,
            CancellationToken ct)
        {
            var allParents = await Task.Run(() => GetAllParents(userId, id), ct);
            var pathUnits = new List<PathUnit>();
            foreach (var entity in allParents)
            {
                pathUnits.Add(new PathUnit
                {
                    Name = entity.Name,
                    Asset = Link.To(nameof(
                        Controllers.AssetsController.GetAssetByIdAsync),
                        new { assetId = entity.Id })
                });
            }

            return pathUnits;
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


        async Task<(bool Succeeded, string Error)>
            IDataService<Asset, AssetEntity>.DeleteResourceAsync(
            string userId,
            string resourceId,
            bool delDependents,
            CancellationToken ct)
        {
            var repo = GetUserRepository(userId, DbContext.Assets);

            if (!delDependents)
            {
                if (repo.Any(a => a.ContainerId == resourceId))
                {
                    return (false, "Content assets found. Cannot delete an asset with dependents.");
                }
                return await DeleteOneResourceAsync(resourceId, repo, ct);
            }

            return await DeleteAssetWithDependentsAsync(resourceId, repo, ct);
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


        #region IDetailDataService METHODS

        async Task<Detail> IDataService<Detail, DetailEntity>.GetResourceAsync(
            string userId,
            string id,
            CancellationToken ct)
        {
            throw new System.NotImplementedException();
        }

        public Task<PagedResults<Detail>> GetResouceCollectionAsync(
            string userId,
            CancellationToken ct,
            PagingOptions pagingOptions = null,
            SortOptions<Detail, DetailEntity> sortOptions = null,
            SearchOptions<Detail, DetailEntity> searchOptions = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<(bool Succeeded, string Error)> AddResourceAsync(
            string userId,
            Detail resource,
            CancellationToken ct)
        {
            throw new NotImplementedException();
        }


        public Task<(bool Succeeded, string Error)> UpdateResourceAsync(string userId, Detail resource, CancellationToken ct)
        {
            throw new NotImplementedException();
        }


        async Task<(bool Succeeded, string Error)>
            IDataService<Detail, DetailEntity>.DeleteResourceAsync(
            string userId,
            string resourceId,
            bool delDependents,
            CancellationToken ct)
        {
            throw new NotImplementedException();
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


        #region ICategoryDataService METHODS

        async Task<Category> IDataService<Category, CategoryEntity>.GetResourceAsync(
            string userId,
            string id,
            CancellationToken ct)
        {
            return await GetOneResourceAsync<Category, CategoryEntity>(
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

        public async Task<(bool Succeeded, string Error)> UpdateResourceAsync(string userId, Category resource, CancellationToken ct)
        {
            return await UpdateOneResourceAsync(
                userId,
                resource,
                DbContext.Categories,
                ct);
        }

        async Task<(bool Succeeded, string Error)>
            IDataService<Category, CategoryEntity>.DeleteResourceAsync(
            string userId,
            string resourceId,
            bool delDependents,
            CancellationToken ct)
        {
            var repo = GetUserRepository(userId, DbContext.Categories);

            var category = repo.SingleOrDefault(c => c.Id == resourceId) ??
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


        #region IUserDataService METHODS

        async Task<User> IDataService<User, UserEntity>.GetResourceAsync(
            string userId,
            string id,
            CancellationToken ct)
        {
            // always pass in developer id
            return await GetOneResourceAsync<User, UserEntity>(
                DbContext.Users,
                ServicesAuthHelper.DeveloperUid,
                id,
                ct);
        }

        public async Task<PagedResults<User>> GetResouceCollectionAsync(
            string userId,
            CancellationToken ct,
            PagingOptions pagingOptions = null,
            SortOptions<User, UserEntity> sortOptions = null,
            SearchOptions<User, UserEntity> searchOptions = null)
        {
            // ignore userId
            return await GetOneTypeResourcesAsync(
                ServicesAuthHelper.DeveloperUid,
                DbContext.Users,
                ct,
                pagingOptions,
                sortOptions,
                searchOptions);
        }

        public async Task<(bool Succeeded, string Error)> AddResourceAsync(
            string userId,
            User resource,
            CancellationToken ct)
        {
            var record = await _userManager.FindByNameAsync(resource.UserName);
            if (record != null)
                return (false, "Existing UserName.");

            record = await _userManager.FindByEmailAsync(resource.Email);
            if (record != null)
                return (false, "Existing Email.");

            var entity = Mapper.Map<User, UserEntity>(resource);
            var result = await _userManager.CreateAsync(entity, resource.Password);
            if (!result.Succeeded)
            {
                var defaultError = result.Errors.FirstOrDefault()?.Description;
                return (false, defaultError);
            }
            return (true, null);
        }

        public Task<(bool Succeeded, string Error)> UpdateResourceAsync(string userId, User resource, CancellationToken ct)
        {
            throw new NotImplementedException();
        }


        async Task<(bool Succeeded, string Error)>
            IDataService<User, UserEntity>.DeleteResourceAsync(
            string userId,
            string resourceId,
            bool delDependents,
            CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> CheckScopedUniquenessAsync(
            string userId,
            PropertyInfo property,
            User resource,
            Scope scope,
            CancellationToken ct)
        {
            return await Task.Run(() =>
                CheckResourceScopedUniqueness(
                    userId,
                    property,
                    resource,
                    scope,
                    DbContext.Users), ct);
        }

        public async Task<User> GetUserAsync(ClaimsPrincipal user)
        {
            return Mapper.Map<UserEntity, User>(await GetUserEntityAsync(user));
        }

        public async Task<User> GetUserByIdAsync(string id)
        {
            return Mapper.Map<UserEntity, User>(await GetUserEntityByIdAsync(id));
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return Mapper.Map<UserEntity, User>(await GetUserEntityByEmailAsync(email));
        }

        public async Task<UserEntity> GetUserEntityAsync(ClaimsPrincipal user)
        {
            return await _userManager.GetUserAsync(user);
        }

        public async Task<UserEntity> GetUserEntityByIdAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public async Task<UserEntity> GetUserEntityByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<string> GetUserIdAsync(ClaimsPrincipal user)
        {
            return await Task.Run(() => _userManager.GetUserId(user));
        }


        #endregion


        #region PRIVATE METHODS

        private IEnumerable<AssetEntity> GetAllParents(
            string userId,
            string id)
        {
            // Unsafe, for admin query
            // [Start of unsafe code]
            if (userId == null)
            {
                var ent = DbContext.Assets.SingleOrDefault(a => a.Id == id);
                if (ent == null) throw new KeyNotFoundException();
                yield return ent;

                // the containerId and the categoryId of the root asset should be null
                while (ent.ContainerId != null && ent.CategoryId != null)
                {
                    ent = DbContext.Assets.SingleOrDefault(a => a.Id == id);
                    if (ent == null) yield break;
                    yield return ent;
                }

                yield break;
            }
            // [End of unsafe code]

            var user = GetUserEntityByIdAsync(userId).GetAwaiter().GetResult();
            if (user == null)
                throw new KeyNotFoundException();

            var repo = GetUserRepository(userId, DbContext.Assets);
            var entity = repo.SingleOrDefault(a => a.Id == id);

            if (entity == null) throw new KeyNotFoundException();
            yield return entity;

            while (!user.RootAssetId.Equals(id))
            {
                entity = repo.SingleOrDefault(a => a.Id == entity.ContainerId);
                if (entity == null) yield break;
                yield return entity;
            }
        }

        private AssetTree ConvertToAssetTree(AssetEntity assetEntity)
        {
            if (assetEntity == null) return null;
            var contents = DbContext.Assets.Where(a => a.ContainerId == assetEntity.Id);

            var assetTree = new AssetTree
            {
                Self = Link.To(nameof(Controllers.AssetTreesController.GetAssetTreeByIdAsync),
                    new { assetTreeId = assetEntity.Id }),
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
            string resourceId,
            IQueryable<AssetEntity> repo,
            CancellationToken ct)
        {
            var contents = repo.Where(a => a.ContainerId == (resourceId));
            if (contents.Any())
            {
                foreach (var asset in contents)
                {
                    var result = await DeleteAssetWithDependentsAsync(asset.Id, repo, ct);
                    if (!result.Succeeded)
                    {
                        return result;
                    }
                }
            }

            return await DeleteOneResourceAsync(resourceId, repo, ct);
        }

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
                var delAsset = await (this as IDataService<Asset, AssetEntity>)
                    .DeleteResourceAsync(category.UserId, asset.Id, true, ct);

                if (!delAsset.Succeeded)
                {
                    return (false, delAsset.Error);
                }
            }

            // TODO: delete base details and their dependents

            return await DeleteOneResourceAsync(category.Id, repo, ct);
        }

        #endregion
    }
}