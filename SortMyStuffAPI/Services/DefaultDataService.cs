using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using SortMyStuffAPI.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace SortMyStuffAPI.Services
{
    public class DefaultDataService : 
        IAssetDataService, 
        IDetailDataService, 
        ICategoryDataService
    {
        private readonly SortMyStuffContext _context;
        private readonly ApiConfigs _apiConfigs;

        public DefaultDataService(SortMyStuffContext context, IOptions<ApiConfigs> apiConfigs)
        {
            _context = context;
            _apiConfigs = apiConfigs.Value;
        }


        #region IAssetDataService METHODS

        public async Task<AssetTree> GetAssetTreeAsync(string id, CancellationToken ct)
        {
            var tree = await Task.Run(() =>
            {
                var entity = _context.Assets.SingleOrDefault(a => a.Id == id);
                return entity == null ? null : ConvertToAssetTree(entity);
            }, ct);

            return tree;
        }

        async Task<Asset> IDataService<Asset, AssetEntity>.GetResourceAsync(string id, CancellationToken ct)
        {
            return await GetResourceAsync<Asset, AssetEntity>(_context.Assets, id, ct);
        }

        public async Task<PagedResults<Asset>> GetResouceCollectionAsync(CancellationToken ct, PagingOptions pagingOptions = null, SortOptions<Asset, AssetEntity> sortOptions = null,
            SearchOptions<Asset, AssetEntity> searchOptions = null)
        {
            return await GetOneTypeResourcesAsync(
                _context.Assets,
                ct,
                pagingOptions,
                sortOptions,
                searchOptions);
        }

        public async Task<IEnumerable<PathUnit>> GetAssetPathAsync(
            string id,
            CancellationToken ct)
        {
            var allParents = await Task.Run(() => GetAllParents(id), ct);
            var pathUnits = new List<PathUnit>();
            foreach (var entity in allParents)
            {
                pathUnits.Add(new PathUnit
                {
                    Name = entity.Name,
                    Asset = Link.To(nameof(Controllers.AssetsController.GetAssetByIdAsync), new { assetId = entity.Id })
                });
            }

            return pathUnits;
        }

        public async Task AddOrUpdateAssetAsync(Asset asset, CancellationToken ct)
        {
            var entity = await Task.Run(() => _context.Assets.SingleOrDefault(a => a.Id == asset.Id), ct);
            if (entity == null)
            {
                _context.Assets.Add(Mapper.Map<Asset, AssetEntity>(asset));
            }
            else
            {
                entity.Name = asset.Name;
                entity.CategoryId = asset.CategoryId;
                entity.ContainerId = asset.ContainerId;
                entity.CreateTimestamp = asset.CreateTimestamp;
                entity.ModifyTimestamp = asset.ModifyTimestamp;
            }

            _context.SaveChanges();
        }

        public async Task DeleteAssetAsync(string id, bool delOnlySelf, CancellationToken ct)
        {
            var delAsset = await Task.Run(() => _context.Assets.SingleOrDefault(a => a.Id == id), ct);
            if (delAsset == null) throw new KeyNotFoundException();

            if (delOnlySelf)
            {
                var contents = await Task.Run(() => _context.Assets.Where(a => a.ContainerId == id), ct);
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
        
        async Task<Detail> IDataService<Detail, DetailEntity>.GetResourceAsync(string id, CancellationToken ct)
        {
            throw new System.NotImplementedException();
        }

        public Task<PagedResults<Detail>> GetResouceCollectionAsync(CancellationToken ct, PagingOptions pagingOptions = null, SortOptions<Detail, DetailEntity> sortOptions = null,
            SearchOptions<Detail, DetailEntity> searchOptions = null)
        {
            throw new System.NotImplementedException();
        }

        #endregion


        #region ICategoryDataService METHODS

        async Task<Category> IDataService<Category, CategoryEntity>.GetResourceAsync(string id, CancellationToken ct)
        {
            return await GetResourceAsync<Category, CategoryEntity>(_context.Categories, id, ct);
        }

        public async Task<PagedResults<Category>> GetResouceCollectionAsync(
            CancellationToken ct, 
            PagingOptions pagingOptions = null, 
            SortOptions<Category, CategoryEntity> sortOptions = null,
            SearchOptions<Category, CategoryEntity> searchOptions = null)
        {
            return await GetOneTypeResourcesAsync(
                _context.Categories,
                ct,
                pagingOptions,
                sortOptions,
                searchOptions);
        }

        public async Task<Category> GetCategoryByNameAsync(string name, CancellationToken ct)
        {
            var entity = await Task.Run(() =>
                _context.Categories.SingleOrDefault(c => 
                    c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)));
            return entity == null ? null : Mapper.Map<CategoryEntity, Category>(entity);
        }

        public async Task AddOrUpdateAssetAsync(Category category, CancellationToken ct)
        {
            var entity = await Task.Run(() => _context.Categories.SingleOrDefault(c => c.Id == category.Id), ct);
            if (entity == null)
            {
                _context.Categories.Add(Mapper.Map<Category, CategoryEntity>(category));
            }
            else
            {
                entity.Name = category.Name;
            }

            _context.SaveChanges();
        }

        public async Task DeleteCategoryAsync(string id, CancellationToken ct)
        {
            var delCategory = await Task.Run(() => _context.Categories.SingleOrDefault(c => c.Id == id), ct);
            if (delCategory == null) throw new KeyNotFoundException();

            // reference key integrity
            if (delCategory.BaseDetails.Any() || delCategory.CategorisedAssets.Any())
                throw new InvalidOperationException(
                    "Cannot delete category when it is referred by BaseDetails or Assets");

            _context.Categories.Remove(delCategory);
            _context.SaveChanges();
        }

        #endregion


        #region PRIVATE METHODS

        private async Task<PagedResults<T>> GetOneTypeResourcesAsync<T, TEntity>(
            IQueryable<TEntity> dbSet,
            CancellationToken ct,
            PagingOptions pagingOptions = null,
            SortOptions<T, TEntity> sortOptions = null,
            SearchOptions<T, TEntity> searchOptions = null) where TEntity : IEntity
        {
            IQueryable<TEntity> query = dbSet;

            if (searchOptions != null)
            {
                query = searchOptions.Apply(query);
            }

            if (sortOptions != null)
            {
                query = sortOptions.Apply(query);
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

        private async Task<T> GetResourceAsync<T, TEntity>(
            IQueryable<TEntity> dbSet,
            string id,
            CancellationToken ct) where TEntity : IEntity
        {
            var entity = await Task.Run(() => dbSet.SingleOrDefault(a => a.Id == id), ct);
            return entity == null ? default(T) : Mapper.Map<TEntity, T>(entity);
        }

        private IEnumerable<AssetEntity> GetAllParents(string id)
        {
            var entity = _context.Assets.SingleOrDefault(a => a.Id == id);
            if (entity == null) throw new KeyNotFoundException();
            yield return entity;

            while (!entity.Id.Equals(_apiConfigs.RootAssetId))
            {
                entity = _context.Assets.SingleOrDefault(a => a.Id == entity.ContainerId);
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

        #endregion
    }
}