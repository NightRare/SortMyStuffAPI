using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using SortMyStuffAPI.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using SortMyStuffAPI.Models.QueryOptions;

namespace SortMyStuffAPI.Services
{
    public class EntityFrameworkDataService : IAssetDataService
    {
        private readonly SortMyStuffContext _context;

        public EntityFrameworkDataService(SortMyStuffContext context)
        {
            _context = context;
        }

        #region IAssetDataService METHODS

        public async Task<AssetTree> GetAssetTreeAsync(string id, CancellationToken ct)
        {
            var entity = await Task.Run(() =>
            _context.AssetTrees.Include(at => at.Contents).SingleOrDefault(at => at.Id == id)
            , ct);
            return entity == null ? null : Mapper.Map<AssetTreeEntity, AssetTree>(entity);
        }

        public async Task<PagedResults<Asset>> GetAllAssetsAsync(
            CancellationToken ct,
            PagingOptions pagingOptions = null,
            SortOptions<Asset, AssetEntity> sortOptions = null,
            SearchOptions<Asset, AssetEntity> searchOptions = null)
        {
            IQueryable<AssetEntity> query = _context.Assets;

            if(searchOptions != null)
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

        public async Task<int> UpdateAssetAsync(
            string id, 
            CancellationToken ct, 
            string name = null, 
            string containerId = null,
            string category = null,
            string modifyTimestamp = null)
        {
            var entity = await Task.Run(() => _context.Assets.SingleOrDefault(a => a.Id == id), ct);
            if (entity == null) return StatusCodes.Status404NotFound;

            entity.Name = name ?? entity.Name;
            entity.ContainerId = containerId ?? entity.ContainerId;
            entity.Category = category ?? entity.Category;
            entity.ModifyTimestamp = modifyTimestamp ?? entity.ModifyTimestamp;

            _context.SaveChanges();

            return StatusCodes.Status200OK;
        }

        #endregion
    }
}