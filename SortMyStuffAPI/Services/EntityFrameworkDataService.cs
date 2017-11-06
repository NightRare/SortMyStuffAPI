using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using SortMyStuffAPI.Models;
using SortMyStuffAPI.Models.Entities;
using SortMyStuffAPI.Models.Resources;

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
            var result = entity == null ? null : Mapper.Map<AssetTreeEntity, AssetTree>(entity);
            return result;
        }

        public Task<IList<Asset>> GetAssets(CancellationToken ct)
        {
            throw new System.NotImplementedException();
        }

        public Task<Asset> GetAsset(string id, CancellationToken ct)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}