﻿using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using SortMyStuffAPI.Models;
using System.Collections.Generic;
using SortMyStuffAPI.Models.QueryOptions;
using SortMyStuffAPI.Utils;
using System.Data.Entity.Migrations;

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

        public async Task AddOrUpdateAssetTreeAsync(AssetTree assetTree, CancellationToken ct)
        {
            var entity = await Task.Run(() =>
                    _context.AssetTrees.Include(at => at.Contents).SingleOrDefault(at => at.Id == assetTree.Id)
                , ct);
            if (entity == null)
            {
                _context.AssetTrees.Add(Mapper.Map<AssetTree, AssetTreeEntity>(assetTree));
            }
            else
            {
                entity.Name = assetTree.Name;

                var idList = assetTree.Contents.Select(at => at.Id);
                entity.Contents = _context.AssetTrees.Include(ate => ate.Contents)
                    .Where(ate => idList.Contains(ate.Id)).ToList();
            }

            _context.SaveChanges();
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

        public async Task<bool> UpdateAssetAsync(
            string id, 
            DateTimeOffset modifyTimestamp,
            CancellationToken ct,
            string name = null, 
            string containerId = null,
            string category = null)
        {
            var entity = await Task.Run(() => _context.Assets.SingleOrDefault(a => a.Id == id), ct);
            if (entity == null) return false;

            entity.Name = name ?? entity.Name;
            entity.ContainerId = containerId ?? entity.ContainerId;
            entity.Category = category ?? entity.Category;
            entity.ModifyTimestamp = modifyTimestamp;

            _context.SaveChanges();

            return true;
        }

        private IEnumerable<AssetEntity> GetAllParents(string id)
        {
            var entity = _context.Assets.SingleOrDefault(a => a.Id == id);
            if (entity == null) throw new KeyNotFoundException();
            yield return entity;
            
            while (!entity.Id.Equals(ApiStrings.ROOT_ASSET_ID))
            {
                entity = _context.Assets.SingleOrDefault(a => a.Id == entity.ContainerId);
                if (entity == null) yield break;
                yield return entity;
            }
        }
        #endregion
    }
}