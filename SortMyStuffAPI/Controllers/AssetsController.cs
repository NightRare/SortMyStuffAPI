using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SortMyStuffAPI.Services;
using SortMyStuffAPI.Models;
using Microsoft.Extensions.Options;
using SortMyStuffAPI.Models.QueryOptions;
using SortMyStuffAPI.Utils;

namespace SortMyStuffAPI.Controllers
{
    [Route("/[controller]")]
    [ApiVersion("0.1")]
    public class AssetsController : Controller
    {

        private readonly IAssetDataService _assetDataService;
        private readonly PagingOptions _defaultpagingOptions;

        public AssetsController(IAssetDataService assetDataService, IOptions<PagingOptions> pagingOptions)
        {
            _assetDataService = assetDataService;
            _defaultpagingOptions = pagingOptions.Value;
        }

        // GET /assets
        [HttpGet(Name = nameof(GetAssetsAsync))]
        public async Task<IActionResult> GetAssetsAsync(
            CancellationToken ct,
            [FromQuery] PagingOptions pagingOptions,
            [FromQuery] SortOptions<Asset, AssetEntity> sortOptions,
            [FromQuery] SearchOptions<Asset, AssetEntity> searchOptions)
        {
            // if any Model (in this case PagingOptions) property is not valid according to the Range attributes
            if (!ModelState.IsValid) return BadRequest(new ApiError(ModelState));

            pagingOptions.Offset = pagingOptions.Offset ?? _defaultpagingOptions.Offset;
            pagingOptions.PageSize = pagingOptions.PageSize ?? _defaultpagingOptions.PageSize;

            var assets = await _assetDataService.GetAllAssetsAsync(ct, pagingOptions, sortOptions, searchOptions);

            var response = PagedCollection<Asset>.Create(
                Link.ToCollection(nameof(GetAssetsAsync)),
                assets.PagedItems.ToArray(),
                assets.TotalSize,
                pagingOptions);

            return Ok(response);
        }

        // GET /assets/{assetId}
        [HttpGet("{assetId}", Name = nameof(GetAssetByIdAsync))]
        public async Task<IActionResult> GetAssetByIdAsync(string assetId, CancellationToken ct)
        {
            var asset = await _assetDataService.GetAssetAsync(assetId, ct);
            if (asset == null) return NotFound();

            return Ok(asset);
        }

        // GET /assets/{assetId}/path
        [HttpGet("{assetId}/path", Name = nameof(GetAssetPathByIDAsync))]
        public async Task<IActionResult> GetAssetPathByIDAsync(
            string assetId,
            CancellationToken ct)
        {
            IEnumerable<PathUnit> pathUnits;
            try
            {
                pathUnits = await _assetDataService.GetAssetPathAsync(assetId, ct);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            var response = new Collection<PathUnit>
            {
                Self = Link.ToCollection(nameof(GetAssetPathByIDAsync), new {assetId = assetId}),
                Value = pathUnits.ToArray()
            };

            return Ok(response);
        }

        // POST /assets
        [HttpPost(Name = nameof(CreateAssetAsync))]
        public async Task<IActionResult> CreateAssetAsync(
            CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        // PUT /assets/{assetId}
        [HttpPut("{assetId}", Name = nameof(AddOrUpdateAssetAsync))]
        public async Task<IActionResult> AddOrUpdateAssetAsync(
            string assetId,
            [FromBody] AddOrUpdateAssetForm form,
            CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(new ApiError(ModelState));

            if (assetId.Equals(ApiStrings.ROOT_ASSET_ID, StringComparison.OrdinalIgnoreCase))
                return BadRequest(new ApiError("Cannot create or modify the root asset."));

            // TODO: check whether category exists

            var container = await _assetDataService.GetAssetAsync(form.ContainerId, ct);
            if (container == null) return BadRequest(new ApiError("Container not found."));

            if (DateTimeOffset.Compare(form.CreateTimestamp.Value, form.ModifyTimestamp.Value) > 0)
                return BadRequest(new ApiError("The create timestamp cannot be later than the modify timestamp."));

            var record = await _assetDataService.GetAssetAsync(assetId, ct);
            if (record == null)
            {
                await CreateAsset(Mapper.Map<AddOrUpdateAssetForm, Asset>(form), ct);
                return Ok();
            }

            if (DateTimeOffset.Compare(record.CreateTimestamp, form.CreateTimestamp.Value) != 0)
                return BadRequest(new ApiError("CreateTimestamp cannot be modified."));

            if (!record.Category.Equals(form.Category))
                return BadRequest(new ApiError("Category cannot be modified."));

            var recordTree = await _assetDataService.GetAssetTreeAsync(assetId, ct);
            recordTree.Name = form.Name;

            // if moving
            if (record.ContainerId != form.ContainerId)
            {
                var fromContainerTree = await _assetDataService.GetAssetTreeAsync(record.ContainerId, ct);
                var toContainerTree = await _assetDataService.GetAssetTreeAsync(form.ContainerId, ct);

                // remove from the fromContainerTree
                fromContainerTree.Contents = fromContainerTree.Contents.Except(
                    fromContainerTree.Contents.Where(at => at.Id == record.Id)).ToArray();

                // add to the toContainerTree
                var toContainerTreeContentsList = toContainerTree.Contents.ToList();
                toContainerTreeContentsList.Add(recordTree);
                toContainerTree.Contents = toContainerTreeContentsList.ToArray();

                await _assetDataService.AddOrUpdateAssetTreeAsync(fromContainerTree, ct);
                await _assetDataService.AddOrUpdateAssetTreeAsync(toContainerTree, ct);
            }

            record.Name = form.Name;
            record.ModifyTimestamp = form.ModifyTimestamp.Value;
            record.ContainerId = form.ContainerId;

            await _assetDataService.AddOrUpdateAssetTreeAsync(recordTree, ct);
            await _assetDataService.AddOrUpdateAssetAsync(record, ct);

            return Ok();
        }

        #region PRIVATE METHODS

        private async Task CreateAsset(Asset newAsset, CancellationToken ct)
        {
            var containerAssetTree = await _assetDataService.GetAssetTreeAsync(newAsset.ContainerId, ct);
            var contentsList = containerAssetTree.Contents.ToList();
            contentsList.Add(new AssetTree
            {
                Id = newAsset.Id,
                Name = newAsset.Name,
                Contents = new AssetTree[0]
            });
            containerAssetTree.Contents = contentsList.ToArray();
            await _assetDataService.AddOrUpdateAssetTreeAsync(containerAssetTree, ct);

            await _assetDataService.AddOrUpdateAssetAsync(newAsset, ct);
        }

        #endregion
    }
}
