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
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace SortMyStuffAPI.Controllers
{
    [Route("/[controller]")]
    [ApiVersion("0.1")]
    public class DocsController : ApiBaseController
    {
        public const string Assets = "assets";
        public const string Categories = "categories";
        public const string BaseDetails = "basedetails";
        public const string Details = "details";

        private readonly IAssetDataService _assetDataService;
        private readonly ICategoryDataService _categoryDataService;
        private readonly IBaseDetailDataService _baseDetailDataService;

        public DocsController(
            IOptions<ApiConfigs> apiConfigs,
            IUserDataService userDataService,
            IHostingEnvironment env,
            IAuthorizationService authService,
            IAssetDataService assetDataService,
            ICategoryDataService categoryDataService,
            IBaseDetailDataService baseDetailDataService)
            : base(userDataService,
                  apiConfigs,
                  env,
                  authService)
        {
            _assetDataService = assetDataService;
            _categoryDataService = categoryDataService;
            _baseDetailDataService = baseDetailDataService;
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
                    Link.ToCollection(
                        nameof(GetDocsByType),
                        new { resourceType = Assets }),
                    Link.ToCollection(
                        nameof(GetDocsByType),
                        new { resourceType = Categories }),
                    Link.ToCollection(
                        nameof(GetDocsByType),
                        new { resourceType = BaseDetails })
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
            switch (resourceType.ToLower())
            {
                case Assets:
                    return Ok(GetAssetDocs());
                case Categories:
                    return Ok(GetCategoryDocs());
                case BaseDetails:
                    return Ok(GetBaseDetailDocs());
                default:
                    return NotFound(new ApiError($"Resource type '{resourceType}' not found."));
            }
        }

        // GET /docs/{resourceType}/{resourceId}
        [Authorize]
        [HttpGet("{resourceType}/{resourceId}", Name = nameof(GetDocsByResourceId))]
        public async Task<IActionResult> GetDocsByResourceId(
            string resourceType,
            string resourceId,
            CancellationToken ct)
        {
            var userId = await GetUserId();

            switch (resourceType.ToLower())
            {
                case Assets:
                    return await _assetDataService.GetResourceAsync(userId, resourceId, ct) == null ?
                        NotFound(new ApiError($"Resource '{resourceType}/{resourceId}' not found.")) :
                        Ok(GetAssetDocs(resourceId)) as IActionResult;

                case Categories:
                    return await _categoryDataService.GetResourceAsync(userId, resourceId, ct) == null ?
                        NotFound(new ApiError($"Resource '{resourceType}/{resourceId}' not found.")) :
                        Ok(GetCategoryDocs(resourceId)) as IActionResult;

                case BaseDetails:
                    return await _baseDetailDataService.GetResourceAsync(userId, resourceId, ct) == null ?
                        NotFound(new ApiError($"Resource '{resourceType}/{resourceId}' not found.")) :
                        Ok(GetBaseDetailDocs(resourceId)) as IActionResult;

                default:
                    return NotFound(new ApiError($"Resource type '{resourceType}' not found."));
            }
        }

        #region PRIVATE METHODS

        private Documentation GetBaseDetailDocs(string baseDetailId = null)
        {
            var list = new List<FormSpecification>();

            var addOrUpdate = FormMetadata.FromModel(
                new BaseDetailForm(),
                Link.ToForm(
                    nameof(BaseDetailsController.AddOrUpdateBaseDetailAsync),
                    new { baseDetailId = baseDetailId ?? "baseDetailId" },
                    ApiStrings.HttpPut,
                    ApiStrings.FormCreateRel, ApiStrings.FormEditRel));
            list.Add(addOrUpdate);

            if (baseDetailId == null)
            {
                var create = FormMetadata.FromModel(
                    new BaseDetailForm(),
                    Link.ToForm(
                        nameof(BaseDetailsController.CreateBaseDetailAsync),
                        null,
                        ApiStrings.HttpPost,
                        ApiStrings.FormCreateRel));
                list.Add(create);
            }

            var response = new Documentation
            {
                Self = GenerateSelfLink(Categories, baseDetailId),
                ResourceType = BaseDetails,
                Value = list.ToArray()
            };

            return response;
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
                Self = GenerateSelfLink(Categories, categoryId),
                ResourceType = Categories,
                Value = list.ToArray()
            };

            return response;
        }

        private static Documentation GetAssetDocs(string assetId = null)
        {
            var list = new List<FormSpecification>();

            var addOrUpdateAsset = FormMetadata.FromModel(
                new FormMode(),
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
                Self = GenerateSelfLink(Assets, assetId),
                ResourceType = Assets,
                Value = list.ToArray()
            };

            return response;
        }

        private static Link GenerateSelfLink(string type, string id)
        {
            return id == null ?
                Link.ToCollection(
                    nameof(GetDocsByType),
                    new
                    {
                        resourceType = type,
                    }) :
                Link.ToCollection(
                    nameof(GetDocsByResourceId),
                    new
                    {
                        resourceType = type,
                        resourceId = id
                    });
        }
    }

    #endregion
}
