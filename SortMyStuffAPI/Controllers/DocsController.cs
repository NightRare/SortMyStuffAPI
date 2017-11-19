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
        private readonly IDetailDataService _detailDataService;

        public DocsController(
            IOptions<ApiConfigs> apiConfigs,
            IUserService userService,
            IHostingEnvironment env,
            IAuthorizationService authService,
            IAssetDataService assetDataService,
            ICategoryDataService categoryDataService,
            IBaseDetailDataService baseDetailDataService,
            IDetailDataService detailDataService)
            : base(userService,
                  apiConfigs,
                  env,
                  authService)
        {
            _assetDataService = assetDataService;
            _categoryDataService = categoryDataService;
            _baseDetailDataService = baseDetailDataService;
            _detailDataService = detailDataService;
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
                    GenerateDocLink(Assets),
                    GenerateDocLink(Categories),
                    GenerateDocLink(BaseDetails),
                    GenerateDocLink(Details)
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
                case Details:
                    return Ok(GetDetailsDocs());
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
                    return await GetResult(
                        _assetDataService, userId, resourceType, 
                        resourceId, GetAssetDocs(resourceId), ct);

                case Categories:
                    return await GetResult(
                        _categoryDataService, userId, resourceType,
                        resourceId, GetCategoryDocs(resourceId), ct);

                case BaseDetails:
                    return await GetResult(
                        _baseDetailDataService, userId, resourceType,
                        resourceId, GetBaseDetailDocs(resourceId), ct);

                case Details:
                    return await GetResult(
                        _detailDataService, userId, resourceType,
                        resourceId, GetDetailsDocs(resourceId), ct);

                default:
                    return NotFound(new ApiError($"Resource type '{resourceType}' not found."));
            }
        }

        #region PRIVATE METHODS

        private async Task<IActionResult> GetResult<T, TEntity>(
            IDataService<T, TEntity> dataService,
            string userId,
            string resourceType,
            string resourceId,
            Documentation doc,
            CancellationToken ct)
            where T : EntityResource
            where TEntity : class, IEntity
        {
            return await dataService.GetResourceAsync(userId, resourceId, ct) == null ?
                NotFound(new ApiError($"Resource '{resourceType}/{resourceId}' not found.")) :
                Ok(doc) as IActionResult;
        }

        private static Documentation GetDetailsDocs(string detailId = null)
        {
            var list = new List<FormSpecification>();

            var update = FormMetadata.FromModel(
                new DetailForm(),
                Link.ToForm(
                    nameof(DetailsController.UpdateDetailAsync),
                    new { detailId = detailId ?? "detailId" },
                    ApiStrings.HttpPatch,
                    ApiStrings.FormEditRel));
            list.Add(update);

            var response = new Documentation
            {
                Self = GenerateDocLink(Details, detailId),
                ResourceType = Details,
                Value = list.ToArray()
            };

            return response;
        }

        private static Documentation GetBaseDetailDocs(string baseDetailId = null)
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
                Self = GenerateDocLink(BaseDetails, baseDetailId),
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
                Self = GenerateDocLink(Categories, categoryId),
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
                Self = GenerateDocLink(Assets, assetId),
                ResourceType = Assets,
                Value = list.ToArray()
            };

            return response;
        }

        private static Link GenerateDocLink(string type, string id = null)
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
