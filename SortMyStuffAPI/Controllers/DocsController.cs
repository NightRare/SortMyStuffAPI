using System;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using SortMyStuffAPI.Infrastructure;
using SortMyStuffAPI.Models;
using SortMyStuffAPI.Utils;

namespace SortMyStuffAPI.Controllers
{
    [Route("/[controller]")]
    [ApiVersion("0.1")]
    public class DocsController : Controller
    {
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
        public IActionResult GetDocsByResourceId(
            string resourceType,
            string resourceId,
            CancellationToken ct)
        {
            if (resourceType.Equals(nameof(Asset), StringComparison.OrdinalIgnoreCase))
            {
                return Ok(GetAssetDocs(nameof(GetDocsByResourceId), resourceId));
            }

            return NotFound();
        }

        private static Documentation GetAssetDocs(string methodName, string assetId = null)
        {
            var addOrUpdateAsset = FormMetadata.FromModel(
                new AddOrUpdateAssetForm(),
                Link.ToForm(
                    nameof(AssetsController.AddOrUpdateAssetAsync),
                    new { assetId = assetId ?? "assetId" },
                    ApiStrings.HTTP_PUT,
                    ApiStrings.FORM_CREATE_REL, ApiStrings.FORM_EDIT_REL));

            var response = new Documentation
            {
                Self = Link.ToCollection(methodName, new { resourceType = nameof(Asset).ToCamelCase() }),
                ResourceType = nameof(Asset).ToCamelCase(),
                Value = new[]
                {
                    addOrUpdateAsset
                }
            };

            return response;
        }
    }
}
