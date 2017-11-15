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
using System.Reflection;
using System.Linq.Expressions;
using SortMyStuffAPI.Exceptions;

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
                userId,
                _context.Assets,
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

        public async Task<(bool Succeeded, string Error)> UpdateResourceAsync(
            string userId,
            Asset resource,
            CancellationToken ct)
        {
            return await UpdateOneResourceAsync(
                userId,
                resource,
                _context.Assets,
                ct);
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
                    _context.Assets), ct);
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
                    _context.Details), ct);
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
                userId,
                _context.Categories,
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

        public async Task<(bool Succeeded, string Error)> UpdateResourceAsync(string userId, Category resource, CancellationToken ct)
        {
            return await UpdateOneResourceAsync(
                userId,
                resource,
                _context.Categories,
                ct);
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
                    _context.Categories), ct);
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
                ServicesAuthHelper.DeveloperUid,
                _context.Users,
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
                    _context.Users), ct);
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
            string userId,
            DbSet<TEntity> dbSet,
            CancellationToken ct,
            PagingOptions pagingOptions = null,
            SortOptions<T, TEntity> sortOptions = null,
            SearchOptions<T, TEntity> searchOptions = null)
            where T : EntityResource
            where TEntity : class, IEntity
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
            where T : EntityResource
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
            where T : EntityResource
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

        private async Task<(bool Succeeded, string Error)> UpdateOneResourceAsync<T, TEntity>(
            string userId,
            T resource,
            DbSet<TEntity> dbSet,
            CancellationToken ct)
            where T : EntityResource
            where TEntity : class, IEntity
        {
            var repo = GetUserRepository(userId, dbSet);
            return await Task.Run(() =>
            {
                var entity = repo.SingleOrDefault(a => a.Id == resource.Id);
                if (entity == null)
                    return (false, $"{resource.GetType().Name} not found");

                var allMutableProperties = entity
                    .GetType()
                    .GetProperties()
                    .Where(p => p.GetCustomAttributes<MutableAttribute>().Any());

                foreach (var prop in allMutableProperties)
                {
                    var updatingValue = resource.GetType()
                        .GetProperty(prop.Name)?
                        .GetValue(resource);

                    if (updatingValue != null)
                        prop.SetValue(entity, updatingValue);
                }

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

        private bool CheckResourceScopedUniqueness<T, TEntity>(
            string userId,
            PropertyInfo property,
            T resource,
            Scope scope,
            DbSet<TEntity> dbSet)
            where T : EntityResource
            where TEntity : class, IEntity
        {
            IQueryable<TEntity> query = dbSet;

            // Trying to build expressions:
            // query = query.Where(e => e.ScopeProperty == ScopeId)
            // query.Any(e => e.Property == resource.Property)

            Expression scopeIdExp;
            PropertyInfo scopeProperty;
            switch (scope)
            {
                case Scope.User:
                    {
                        // Get the value of resource.UserId (i.e. ScopeId)
                        scopeIdExp = Expression.Constant(resource.UserId ?? userId) ??
                            throw new ApiException(
                                "Unable to get the value of userId");

                        // ?.UserId (i.e. ?.ScopeProperty)
                        scopeProperty = ExpressionHelper.GetPropertyInfo<TEntity>("UserId") ??
                            throw new ApiException(
                                "Unable to locate the scope. 'UserId' property not found.");
                        break;
                    }
                case Scope.Category:
                    {
                        // ?.CategoryId (i.e. ?.ScopeProperty)
                        scopeProperty = resource.GetType().GetProperty("CategoryId") ?? 
                            throw new ApiException(
                                "Unable to locate the scope. 'CategoryId' property not found.");

                        // Get the value of resource.CategoryId (i.e. ScopeId)
                        scopeIdExp = Expression.Constant(scopeProperty.GetValue(resource)) ?? 
                            throw new ApiException(
                                "Unable to get the value of categoryId");

                        break;
                    }
                default:
                    throw new ApiException("Unexpected scope");
            }

            // stage 1: query = dbSet.Where(e => e.UserId == resource.UserId)

            // e
            var entityObj = Expression.Parameter(typeof(TEntity), "e");

            // e.ScopeProperty
            var entityScopeProperty = ExpressionHelper.GetPropertyExpression(
                entityObj, scopeProperty);

            // e.ScopeProperty == ScopeId
            var comparisonScopeId = Expression.Equal(
                entityScopeProperty, scopeIdExp);

            // e => e.UserId == resource.UserId
            var lambdaComparisonScopeId = ExpressionHelper
                .GetLambda<TEntity, bool>(entityObj, comparisonScopeId);

            // query = query.Where...
            query = ExpressionHelper.CallWhere(query, lambdaComparisonScopeId);

            // then check uniqueness
            // stage 2: query.Any(e => e.Property == resource.Property)

            // e.Property
            var entityProperty = ExpressionHelper.GetPropertyExpression(
                entityObj, typeof(TEntity).GetProperty(property.Name));

            // the value of resource.Property
            var resourcePropertyValue = resource.GetType()
                .GetProperty(property.Name)?.GetValue(resource) ??
                throw new ApiException(
                    $"Check scoped uniqueness failed. " +
                    $"The value of {property.Name} should not be null.");
            var resourceProperty = Expression.Constant(resourcePropertyValue);

            // e.Property == resource.Property
            var comparisonProperty = Expression.Equal(
                entityProperty, resourceProperty);

            // e => e.Property == resource.Property
            var lambdaComparisonProperty = ExpressionHelper.GetLambda<TEntity, bool>(
                entityObj, comparisonProperty);

            // query.Any(...)
            return !ExpressionHelper.CallAny(query, lambdaComparisonProperty);
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