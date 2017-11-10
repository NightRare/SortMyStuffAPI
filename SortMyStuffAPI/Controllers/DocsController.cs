using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SortMyStuffAPI.Infrastructure;
using SortMyStuffAPI.Models;
using SortMyStuffAPI.Services;
using SortMyStuffAPI.Utils;

namespace SortMyStuffAPI.Controllers
{
    [Route("/[controller]")]
    [ApiVersion("0.1")]
    public class DocsController : Controller
    {
        private readonly IAssetDataService _assetDataService;

        public DocsController(IAssetDataService assetDataService)
        {
            _assetDataService = assetDataService;
        }

        // GET /docs
        [HttpGet(Name = nameof(GetDocs))]
        public IActionResult GetDocs()
        {
            var response = new Collection<Link>
            {
                Self = Link.ToCollection(nameof(GetDocs)),
                Value = new[]
                {
                    Link.ToCollection(nameof(GetDocsByType), new { resourceType = nameof(Asset).ToCamelCase() })
                }
            };

            return Ok(response);
        }


        // GET /docs/{resourceType}
        [HttpGet("{resourceType}", Name = nameof(GetDocsByType))]
        public IActionResult GetDocsByType(
            string resourceType, 
            CancellationToken ct)
        {
            if (resourceType.Equals(nameof(Asset), StringComparison.OrdinalIgnoreCase))
            {
                return Ok(GetAssetDocs(nameof(GetDocsByType)));
            }

            return NotFound();
        }

        // GET /docs/{resourceType}/{resourceId}
        [HttpGet("{resourceType}/{resourceId}", Name = nameof(GetDocsByResourceId))]
        public async Task<IActionResult> GetDocsByResourceId(
            string resourceType,
            string resourceId,
            CancellationToken ct)
        {
            if (resourceType.Equals(nameof(Asset), StringComparison.OrdinalIgnoreCase))
            {
                if (await _assetDataService.GetAssetAsync(resourceId, ct) == null)
                    return NotFound(new ApiError($"Resource '{resourceType}/{resourceId}' not found."));

                return Ok(GetAssetDocs(nameof(GetDocsByResourceId), resourceId));
            }

            return NotFound(new ApiError($"Resource type '{resourceType}' not found."));
        }

        private static Documentation GetAssetDocs(string methodName, string assetId = null)
        {
            var list = new List<FormSpecification>();

            var addOrUpdateAsset = FormMetadata.FromModel(
                new AddOrUpdateAssetForm(),
                Link.ToForm(
                    nameof(AssetsController.AddOrUpdateAssetAsync),
                    new { assetId = assetId ?? "assetId" },
                    ApiStrings.HttpPut,
                    ApiStrings.FormCreateRel, ApiStrings.FormEditRel));
            list.Add(addOrUpdateAsset);

            if (assetId == null)
            {
                var createAsset = FormMetadata.FromModel(
                    new CreateAssetForm(),
                    Link.ToForm(
                        nameof(AssetsController.CreateAssetAsync),
                        null,
                        ApiStrings.HttpPost,
                        ApiStrings.FormCreateRel));
                list.Add(createAsset);
            }

            var response = new Documentation
            {
                Self = Link.ToCollection(methodName, new { resourceType = nameof(Asset).ToCamelCase() }),
                ResourceType = nameof(Asset).ToCamelCase(),
                Value = list.ToArray()
            };

            return response;
        }
    }
}
