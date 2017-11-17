using System;
using AutoMapper;
using SortMyStuffAPI.Models;
using SortMyStuffAPI.Utils;

namespace SortMyStuffAPI.Infrastructure
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            #region Asset

            CreateMap<AssetEntity, Asset>()
                .ForMember(dest => dest.Self, opt => opt.MapFrom(src =>
                    Link.To(nameof(Controllers.AssetsController.GetAssetByIdAsync), new { assetId = src.Id })))

                .ForMember(dest => dest.Category, opt => opt.MapFrom(src =>
                    Link.To(nameof(Controllers.CategoriesController.GetCategoryByIdAsync), new { categoryId = src.CategoryId })))

                .ForMember(dest => dest.Path, opt => opt.MapFrom(src =>
                    Link.ToCollection(nameof(Controllers.AssetsController.GetAssetPathByIdAsync), new { assetId = src.Id })))

                .ForMember(dest => dest.AssetTree, opt => opt.MapFrom(src =>
                    Link.To(nameof(Controllers.AssetTreesController.GetAssetTreeByIdAsync), new { assetId = src.Id })))

                .ForMember(dest => dest.Thumbnail, opt => opt.MapFrom(src =>
                    Link.To(nameof(Controllers.ThumbnailsController.GetThumbnailByIdAsync), new { assetId = src.Id })))

                .ForMember(dest => dest.Photo, opt => opt.MapFrom(src =>
                    Link.To(nameof(Controllers.PhotosController.GetPhotoByIdAsync), new { assetId = src.Id })))

                .ForMember(dest => dest.ContentAssets, opt => opt.MapFrom(src =>
                    Link.ToCollection(
                        nameof(Controllers.AssetsController.GetAssetsAsync),
                        new { search = $"{ nameof(Asset.ContainerId).ToCamelCase() } { ApiStrings.ParameterOpEqual } { src.Id }" })))

                .ForMember(dest => dest.Details, opt => opt.MapFrom(src =>
                    Link.ToCollection(nameof(Controllers.DetailsController.GetDetailsAsync),
                        new { search = $"{ nameof(Detail.AssetId).ToCamelCase() } { ApiStrings.ParameterOpEqual } { src.Id }" })))

                .ForMember(dest => dest.FormSpecs, opt => opt.MapFrom(src =>
                    src.Id == "rootassetid" ?
                    null : Link.ToCollection(
                            nameof(Controllers.DocsController.GetDocsByResourceId),
                            new { resourceType = Controllers.DocsController.Assets, resourceId = src.Id })));

            CreateMap<Asset, AssetEntity>();

            CreateMap<FormMode, Asset>();

            CreateMap<CreateAssetForm, Asset>();

            #endregion


            #region Category

            CreateMap<CategoryEntity, Category>()
                .ForMember(dest => dest.Self, opt => opt.MapFrom(src =>
                    Link.To(nameof(Controllers.CategoriesController.GetCategoryByIdAsync), 
                        new { categoryId = src.Id })))

                .ForMember(dest => dest.BaseDetails, opt => opt.MapFrom(src =>
                    Link.ToCollection(nameof(Controllers.BaseDetailsController.GetBaseDetailsAsync),
                        new { search = $"{ nameof(BaseDetail.CategoryId).ToCamelCase() } " +
                            $"{ ApiStrings.ParameterOpEqual } { src.Id }" })))

                .ForMember(dest => dest.CategorisedAssets, opt => opt.MapFrom(src =>
                    Link.ToCollection(nameof(Controllers.AssetsController.GetAssetsAsync),
                        new { search = $"{ nameof(Asset.CategoryId).ToCamelCase() } " +
                            $"{ ApiStrings.ParameterOpEqual } { src.Id }" })))

                .ForMember(dest => dest.FormSpecs, opt => opt.MapFrom(src =>
                    Link.ToCollection(
                            nameof(Controllers.DocsController.GetDocsByResourceId),
                            new { resourceType = Controllers.DocsController.Categories,
                                resourceId = src.Id })));

            CreateMap<Category, CategoryEntity>()
                .ForMember(dest => dest.BaseDetails, opt => opt.Ignore())
                .ForMember(dest => dest.CategorisedAssets, opt => opt.Ignore());

            CreateMap<CategoryForm, Category>();

            #endregion


            #region User

            CreateMap<UserEntity, User>()
                .ForMember(dest => dest.Self, opt => opt.MapFrom(src =>
                    Link.To(nameof(Controllers.UsersController.GetUserByIdAsync), new { userId = src.Id })))

                .ForMember(dest => dest.Provider, opt => opt.MapFrom(src =>
                    src.Provider.ToString()));

            CreateMap<User, UserEntity>()
                .ForMember(dest => dest.Provider, opt => opt.MapFrom(src =>
                    Enum.Parse<AuthProvider>(src.Provider)));

            CreateMap<RegisterForm, User>();

            #endregion


            #region BaseDetail

            CreateMap<BaseDetail, BaseDetailEntity>()
                .ForMember(dest => dest.Style, opt => opt.MapFrom(src =>
                    Enum.Parse<DetailStyle>(src.Style)))

                .ForMember(dest => dest.Category, opt => opt.Ignore())

                .ForMember(dest => dest.Derivatives, opt => opt.Ignore());

            CreateMap<BaseDetailEntity, BaseDetail>()
                .ForMember(dest => dest.Self, opt => opt.MapFrom(src =>
                    Link.To(nameof(Controllers.BaseDetailsController.GetBaseDetailByIdAsync), new { baseDetailId = src.Id })))

                .ForMember(dest => dest.Style, opt => opt.MapFrom(src =>
                    src.Style.ToString()))

                .ForMember(dest => dest.Category, opt => opt.MapFrom(src =>
                    Link.To(nameof(Controllers.CategoriesController.GetCategoryByIdAsync),
                    new { categoryId = src.CategoryId })))

                .ForMember(dest => dest.Derivatives, opt => opt.MapFrom(src =>
                    Link.ToCollection(nameof(Controllers.DetailsController.GetDetailsAsync),
                    new { search = $"{ nameof(Detail.BaseId).ToCamelCase() } { ApiStrings.ParameterOpEqual } { src.Id }" })))

                .ForMember(dest => dest.FormSpecs, opt => opt.MapFrom(src =>
                    Link.ToCollection(
                        nameof(Controllers.DocsController.GetDocsByResourceId),
                        new { resourceType = Controllers.DocsController.BaseDetails, resourceId = src.Id })));

            CreateMap<BaseDetailForm, BaseDetail>();

            #endregion
        }
    }
}
