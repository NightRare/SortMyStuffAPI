using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using SortMyStuffAPI.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SortMyStuffAPI.Infrastructure;
using System.Security.Claims;
using SortMyStuffAPI.Utils;
using Microsoft.EntityFrameworkCore;

namespace SortMyStuffAPI.Services
{
    public class DefaultDataService :
        IAssetDataService,
        IDetailDataService,
        ICategoryDataService,
        IUserDataService
    {
        private readonly SortMyStuffContext _context;
        private readonly ApiConfigs _apiConfigs;
        private readonly UserManager<UserEntity> _userManager;
        private readonly RoleManager<UserRoleEntity> _roleManager;

        public DefaultDataService(
            SortMyStuffContext context,
            IOptions<ApiConfigs> apiConfigs,
            UserManager<UserEntity> userManager,
            RoleManager<UserRoleEntity> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _apiConfigs = apiConfigs.Value;
        }


        #region IAssetDataService METHODS

        public async Task<AssetTree> GetAssetTreeAsync(
            string userId,
            string id,
            CancellationToken ct)
        {
            var repo = GetUserRepository(userId, _context.Assets);
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
                _context.Assets,
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
                _context.Assets,
                userId,
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
                _context.Assets, 
                ct);
        }

        public async Task<(bool Succeeded, string Error)> UpdateAssetAsync(
            string userId,
            Asset asset,
            CancellationToken ct)
        {
            var repo = GetUserRepository(userId, _context.Assets);
            return await Task.Run(() =>
            {
                var entity = repo.SingleOrDefault(a => a.Id == asset.Id);
                if (entity == null)
                    return (false, "Asset not found");

                entity.Name = asset.Name;
                entity.CategoryId = asset.CategoryId;
                entity.ContainerId = asset.ContainerId;
                entity.CreateTimestamp = asset.CreateTimestamp;
                entity.ModifyTimestamp = asset.ModifyTimestamp;

                try
                {
                    _context.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    return (false, ex.Message);
                }
                return (true, null);
            }, ct);
        }

        public async Task DeleteAssetAsync(
            string userId,
            string id,
            bool delOnlySelf,
            CancellationToken ct)
        {
            var repo = GetUserRepository(userId, _context.Assets);
            var delAsset = await Task.Run(() => repo.SingleOrDefault(a => a.Id == id), ct);
            if (delAsset == null) throw new KeyNotFoundException();

            if (delOnlySelf)
            {
                var contents = await Task.Run(() => repo.Where(a => a.ContainerId == id), ct);
                foreach (var asset in contents)
                {
                    asset.ContainerId = delAsset.ContainerId;
                }

                _context.Assets.Remove(delAsset);
                _context.SaveChanges();
                return;
            }

            DeleteAsset(delAsset);
            _context.SaveChanges();
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

        #endregion


        #region ICategoryDataService METHODS

        async Task<Category> IDataService<Category, CategoryEntity>.GetResourceAsync(
            string userId,
            string id,
            CancellationToken ct)
        {
            return await GetOneResourceAsync<Category, CategoryEntity>(
                _context.Categories,
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
                _context.Categories,
                userId,
                ct,
                pagingOptions,
                sortOptions,
                searchOptions);
        }

        public async Task<Category> GetCategoryByNameAsync(
            string userId,
            string name,
            CancellationToken ct)
        {
            var repo = GetUserRepository(userId, _context.Categories);

            var entity = await Task.Run(() =>
                repo.SingleOrDefault(c =>
                    c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)));
            return entity == null ? null : Mapper.Map<CategoryEntity, Category>(entity);
        }

        public async Task<(bool Succeeded, string Error)> AddResourceAsync(
            string userId, 
            Category resource, 
            CancellationToken ct)
        {
            return await AddOneResourceAsync(
                userId,
                resource,
                _context.Categories,
                ct);
        }

        public async Task<(bool Succeeded, string Error)> UpdateCategoryAsync(
            string userId,
            Category catgeory,
            CancellationToken ct)
        {
            var repo = GetUserRepository(userId, _context.Categories);
            return await Task.Run(() =>
            {
                var entity = repo.SingleOrDefault(c => c.Id == catgeory.Id);
                if (entity == null)
                    return (false, "Category not found");

                entity.Name = catgeory.Name;

                try
                {
                    _context.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    return (false, ex.Message);
                }
                return (true, null);
            }, ct);
        }

        public async Task DeleteCategoryAsync(
            string userId,
            string id,
            CancellationToken ct)
        {
            var repo = GetUserRepository(userId, _context.Categories);
            var delCategory = await Task.Run(() => repo.SingleOrDefault(c => c.Id == id), ct);
            if (delCategory == null) throw new KeyNotFoundException();

            // reference key integrity
            if (delCategory.BaseDetails.Any() || delCategory.CategorisedAssets.Any())
                throw new InvalidOperationException(
                    "Cannot delete category when it is referred by BaseDetails or Assets");

            _context.Categories.Remove(delCategory);
            _context.SaveChanges();
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
                _context.Users,
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
                _context.Users,
                ServicesAuthHelper.DeveloperUid,
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

        private async Task<PagedResults<T>> GetOneTypeResourcesAsync<T, TEntity>(
            IQueryable<TEntity> dbSet,
            string userId,
            CancellationToken ct,
            PagingOptions pagingOptions = null,
            SortOptions<T, TEntity> sortOptions = null,
            SearchOptions<T, TEntity> searchOptions = null)
            where TEntity : IEntity
        {
            IQueryable<TEntity> query = GetUserRepository(userId, dbSet);

            if (searchOptions != null)
            {
                query = new SearchOptionsProcessor<T, TEntity>(searchOptions)
                    .Apply(query);
            }

            if (sortOptions != null)
            {
                query = new SortOptionsProcessor<T, TEntity>(sortOptions)
                    .Apply(query);
            }

            IEnumerable<T> resources = await Task.Run(
                () => query.ProjectTo<T>().ToArray(),
                ct);

            var totalSize = resources.Count();

            if (pagingOptions != null)
            {
                var pagedResources = resources
                    .Skip(pagingOptions.Offset.Value)
                    .Take(pagingOptions.PageSize.Value);
                resources = pagedResources;
            }

            return new PagedResults<T>
            {
                PagedItems = resources,
                TotalSize = totalSize
            };
        }

        private async Task<T> GetOneResourceAsync<T, TEntity>(
            IQueryable<TEntity> dbSet,
            string userId,
            string id,
            CancellationToken ct)
            where TEntity : IEntity
        {
            var repo = GetUserRepository(userId, dbSet);

            var entity = await Task.Run(() => repo.SingleOrDefault(a => a.Id == id), ct);
            return entity == null ? default(T) : Mapper.Map<TEntity, T>(entity);
        }

        private async Task<(bool Succeeded, string Error)> AddOneResourceAsync<T, TEntity>(
            string userId,
            T resource,
            DbSet<TEntity> dbSet,
            CancellationToken ct)
            where T : Resource
            where TEntity : class, IEntity
        {
            if (userId == null)
                return (false, "The userId cannot be null.");

            var repo = GetUserRepository(userId, dbSet);

            var entity = Mapper.Map<T, TEntity>(resource);
            entity.UserId = userId;

            if (repo.Any(e => e.Id == entity.Id))
                return (false, $"{resource.GetType().Name} already exists");

            return await Task.Run(() =>
            {
                dbSet.Add(entity);
                try
                {
                    _context.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    return (false, ex.Message);
                }

                return (true, null);
            }, ct);
        }

        private IEnumerable<AssetEntity> GetAllParents(
            string userId,
            string id)
        {
            // Unsafe, for admin query
            // [Start of unsafe code]
            if (userId == null)
            {
                var ent = _context.Assets.SingleOrDefault(a => a.Id == id);
                if (ent == null) throw new KeyNotFoundException();
                yield return ent;

                // the containerId and the categoryId of the root asset should be null
                while (ent.ContainerId != null && ent.CategoryId != null)
                {
                    ent = _context.Assets.SingleOrDefault(a => a.Id == id);
                    if (ent == null) yield break;
                    yield return ent;
                }

                yield break;
            }
            // [End of unsafe code]

            var user = GetUserEntityByIdAsync(userId).GetAwaiter().GetResult();
            if (user == null)
                throw new KeyNotFoundException();

            var repo = GetUserRepository(userId, _context.Assets);
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
            var contents = _context.Assets.Where(a => a.ContainerId == assetEntity.Id);

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
            var contents = _context.Assets.Where(a => a.ContainerId == entity.Id);
            foreach (var asset in contents)
            {
                DeleteAsset(asset);
            }
            _context.Assets.Remove(entity);
        }

        private IQueryable<TEntity> GetUserRepository<TEntity>(
            string userId,
            IQueryable<TEntity> dbSet)
            where TEntity : IEntity
        {
            if (userId == null)
                throw new ArgumentException("The userId cannot be null.");

            if (ServicesAuthHelper.IsDeveloper(userId))
                return dbSet;

            return dbSet.Where(e => e.UserId == userId);
        }

        #endregion
    }
}