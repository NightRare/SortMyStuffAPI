using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SortMyStuffAPI.Infrastructure;
using SortMyStuffAPI.Models;
using SortMyStuffAPI.Services;
using SortMyStuffAPI.Utils;
using Microsoft.AspNetCore.Authorization;

namespace SortMyStuffAPI.Controllers
{
    [Route("/[controller]")]
    [ApiVersion("0.1")]
    public class DocsController : Controller
    {
        public const string AssetsTypeName = "assets";
        public const string CategoriesTypeName = "categories";

        private readonly IAssetDataService _assetDataService;
        private readonly ICategoryDataService _categoryDataService;

        public DocsController(
            IAssetDataService assetDataService,
            ICategoryDataService categoryDataService)
        {
            _assetDataService = assetDataService;
            _categoryDataService = categoryDataService;
        }

        // GET /docs
        [Authorize]
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
        [Authorize]
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
        [Authorize]
        [HttpGet("{resourceType}/{resourceId}", Name = nameof(GetDocsByResourceId))]
        public async Task<IActionResult> GetDocsByResourceId(
            string resourceType,
            string resourceId,
            CancellationToken ct)
        {
            switch (resourceType.ToLower())
            {
                case AssetsTypeName:
                    {
                        return await _assetDataService.GetResourceAsync(resourceId, ct) == null ?
                            NotFound(new ApiError($"Resource '{resourceType}/{resourceId}' not found.")) :
                            Ok(GetAssetDocs(resourceId)) as IActionResult;
                    }
                case CategoriesTypeName:
                    {
                        return await _categoryDataService.GetResourceAsync(resourceId, ct) == null ?
                            NotFound(new ApiError($"Resource '{resourceType}/{resourceId}' not found.")) :
                            Ok(GetCategoryDocs(resourceId)) as IActionResult;
                    }
                default:
                    {
                        return NotFound(new ApiError($"Resource type '{resourceType}' not found."));
                    }
            }
        }

        private static Documentation GetCategoryDocs(string categoryId = null)
        {
            var list = new List<FormSpecification>();

            var addOrUpdate = FormMetadata.FromModel(
                new CategoryForm(),
                Link.ToForm(
                    nameof(CategoriesController.AddOrUpdateCategoryAsync),
                    new { categoryId = categoryId ?? "categoryId" },
                    ApiStrings.HttpPut,
                    ApiStrings.FormCreateRel, ApiStrings.FormEditRel));
            list.Add(addOrUpdate);

            if (categoryId == null)
            {
                var create = FormMetadata.FromModel(
                    new CategoryForm(),
                    Link.ToForm(
                        nameof(CategoriesController.CreateCatgegoryAsync),
                        null,
                        ApiStrings.HttpPost,
                        ApiStrings.FormCreateRel));
                list.Add(create);
            }

            var response = new Documentation
            {
                Self = Link.ToCollection(
                    nameof(GetDocsByResourceId), 
                    new { resourceType = CategoriesTypeName }),
                ResourceType = CategoriesTypeName,
                Value = list.ToArray()
            };

            return response;
        }

        private static Documentation GetAssetDocs(string assetId = null)
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
                Self = Link.ToCollection(
                    nameof(GetDocsByResourceId), 
                    new { resourceType = AssetsTypeName }),
                ResourceType = AssetsTypeName,
                Value = list.ToArray()
            };

            return response;
        }
    }
}
