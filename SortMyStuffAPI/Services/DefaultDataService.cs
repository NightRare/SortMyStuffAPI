using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using SortMyStuffAPI.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using SortMyStuffAPI.Infrastructure;
using SortMyStuffAPI.Models.QueryOptions;

namespace SortMyStuffAPI.Services
{
    public class DefaultDataService : IAssetDataService
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
                return ConvertToAssetTree(entity);
            }, ct);

            return tree;
        }

        public async Task<PagedResults<Asset>> GetAllAssetsAsync(
            CancellationToken ct,
            PagingOptions pagingOptions = null,
            SortOptions<Asset, AssetEntity> sortOptions = null,
            SearchOptions<Asset, AssetEntity> searchOptions = null)
        {
            IQueryable<AssetEntity> query = _context.Assets;

            if (searchOptions != null)
            {
                query = searchOptions.Apply(query);
            }

            if (sortOptions != null)
            {
                query = sortOptions.Apply(query);
            }

            IEnumerable<Asset> assets = await Task.Run(
                () => query.ProjectTo<Asset>().ToArray(),
                ct);
            var totalSize = assets.Count();

            if (pagingOptions != null)
            {
                var pagedAssets = assets
                    .Skip(pagingOptions.Offset.Value)
                    .Take(pagingOptions.PageSize.Value);
                assets = pagedAssets;
            }

            return new PagedResults<Asset>
            {
                PagedItems = assets,
                TotalSize = totalSize
            };
        }

        public async Task<Asset> GetAssetAsync(string id, CancellationToken ct)
        {
            var entity = await Task.Run(() => _context.Assets.SingleOrDefault(a => a.Id == id), ct);
            return entity == null ? null : Mapper.Map<AssetEntity, Asset>(entity);
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
                entity.Category = asset.Category;
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

        #region PRIVATE METHODS

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