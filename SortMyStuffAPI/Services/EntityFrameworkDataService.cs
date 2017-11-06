using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using SortMyStuffAPI.Models.Entities;
using SortMyStuffAPI.Models.Resources;
using AutoMapper.QueryableExtensions;

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

        public async Task<IEnumerable<Asset>> GetAssetsAsync(CancellationToken ct)
        {
            var result = await Task.Run(() => _context.Assets.ProjectTo<Asset>());
            return result;
        }

        public async Task<Asset> GetAssetAsync(string id, CancellationToken ct)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}