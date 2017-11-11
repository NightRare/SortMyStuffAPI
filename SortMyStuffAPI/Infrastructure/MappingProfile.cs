using AutoMapper;
using SortMyStuffAPI.Models;
using SortMyStuffAPI.Utils;

namespace SortMyStuffAPI.Infrastructure
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<AssetEntity, Asset>()
                .ForMember(dest => dest.Self, opt => opt.MapFrom(src =>
                    Link.To(nameof(Controllers.AssetsController.GetAssetByIdAsync), new { assetId = src.Id })))

                .ForMember(dest => dest.Category, opt => opt.MapFrom(src =>
                    Link.To(nameof(Controllers.CategoriesController.GetCategoryByIdAsync), new { categoryId = src.CategoryId })))

                .ForMember(dest => dest.Path, opt => opt.MapFrom(src =>
                    Link.ToCollection(nameof(Controllers.AssetsController.GetAssetPathByIdAsync), new { assetId = src.Id })))

                .ForMember(dest => dest.Thumbnail, opt => opt.MapFrom(src =>
                    Link.To(nameof(Controllers.ThumbnailsController.GetThumbnailByIdAsync), new { assetId = src.Id })))

                .ForMember(dest => dest.Photo, opt => opt.MapFrom(src =>
                    Link.To(nameof(Controllers.PhotosController.GetPhotoByIdAsync), new { assetId = src.Id })))

                .ForMember(dest => dest.ContentAssets, opt => opt.MapFrom(src =>
                    Link.ToCollection(
                        nameof(Controllers.AssetsController.GetAssetsAsync),
                        new { search = $"{ nameof(Asset.ContainerId).ToCamelCase() } { ApiStrings.ParameterOpEqual } { src.Id }" })))

                .ForMember(dest => dest.FormSpecs, opt => opt.MapFrom(src => 
                    src.Id == "rootassetid" ? 
                    null : Link.ToCollection(
                            nameof(Controllers.DocsController.GetDocsByResourceId),
                            new { resourceType = Controllers.DocsController.AssetsTypeName, resourceId = src.Id })));

            CreateMap<Asset, AssetEntity>();

            CreateMap<AddOrUpdateAssetForm, Asset>();

            CreateMap<CreateAssetForm, Asset>();

            CreateMap<CategoryEntity, Category>()
                .ForMember(dest => dest.Self, opt => opt.MapFrom(src =>
                    Link.To(nameof(Controllers.CategoriesController.GetCategoryByIdAsync), new {categoryId = src.Id})))

                // TODO: change BaseDetails mapping
                .ForMember(dest => dest.BaseDetails, opt => opt.MapFrom(src =>
                    Link.ToCollection(nameof(Controllers.CategoriesController.GetCategoryByIdAsync), null)))

                .ForMember(dest => dest.CategorisedAssets, opt => opt.MapFrom(src =>
                    Link.ToCollection(nameof(Controllers.AssetsController.GetAssetsAsync),
                        new { search = $"{ nameof(Asset.CategoryId).ToCamelCase() } { ApiStrings.ParameterOpEqual } { src.Id }" })))

                .ForMember(dest => dest.FormSpecs, opt => opt.MapFrom(src =>
                    Link.ToCollection(
                            nameof(Controllers.DocsController.GetDocsByResourceId),
                            new { resourceType = Controllers.DocsController.CategoriesTypeName, resourceId = src.Id })));

            CreateMap<Category, CategoryEntity>()
                .ForMember(dest => dest.BaseDetails, opt => opt.Ignore())
                .ForMember(dest => dest.CategorisedAssets, opt => opt.Ignore());

            CreateMap<CategoryForm, Category>();
        }
    }
}
