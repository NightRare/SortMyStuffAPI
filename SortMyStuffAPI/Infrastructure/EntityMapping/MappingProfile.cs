using System;
using AutoMapper;
using SortMyStuffAPI.Models;
using SortMyStuffAPI.Utils;
using Newtonsoft.Json;

namespace SortMyStuffAPI.Infrastructure
{
    public static class EntityToResourceExtensions
    {
        public static object ToResource(this IEntity entity)
        {
            if (entity is AssetEntity)
            {
                return Mapper.Map<Asset>(entity);
            }
            else if (entity is BaseDetailEntity)
            {
                return Mapper.Map<BaseDetail>(entity);
            }
            else if (entity is CategoryEntity)
            {
                return Mapper.Map<Category>(entity);
            }
            else if (entity is DetailEntity)
            {
                return Mapper.Map<Detail>(entity);
            }
            else if(entity is UserEntity)
            {
                return Mapper.Map<User>(entity);
            }
            return null;
        }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            #region Asset

            CreateMap<AssetEntity, Asset>()
                .ForMember(dest => dest.Self, opt => opt.MapFrom(src =>
                    Link.To(nameof(Controllers.AssetsController.GetAssetByIdAsync), 
                    new { assetId = src.Id })))

                .ForMember(dest => dest.ContainerId, opt => opt.MapFrom(src =>
                    src.ContainerId.Equals(ApiStrings.RootAssetToken) ? null : src.ContainerId))

                .ForMember(dest => dest.Category, opt => opt.MapFrom(src =>
                    Link.To(nameof(Controllers.CategoriesController.GetCategoryByIdAsync), 
                    new { categoryId = src.CategoryId })))

                .ForMember(dest => dest.Path, opt => opt.MapFrom(src =>
                    Link.ToCollection(nameof(Controllers.AssetsController.GetAssetPathByIdAsync), 
                    new { assetId = src.Id })))

                .ForMember(dest => dest.AssetTree, opt => opt.MapFrom(src =>
                    Link.To(nameof(Controllers.AssetTreesController.GetAssetTreeByIdAsync), 
                    new { assetId = src.Id })))

                .ForMember(dest => dest.Thumbnail, opt => opt.MapFrom(src =>
                    Link.To(nameof(Controllers.ThumbnailsController.GetThumbnailByIdAsync), 
                    new { assetId = src.Id })))

                .ForMember(dest => dest.Photo, opt => opt.MapFrom(src =>
                    Link.To(nameof(Controllers.PhotosController.GetPhotoByIdAsync), 
                    new { assetId = src.Id })))

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

            CreateMap<Asset, AssetEntity>()
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Details, opt => opt.Ignore());

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

            CreateMap<BaseDetail, BaseDetailEntity>()
                .ForMember(dest => dest.Style, opt => opt.MapFrom(src =>
                    Enum.Parse<DetailStyle>(src.Style)))

                .ForMember(dest => dest.Category, opt => opt.Ignore())

                .ForMember(dest => dest.Derivatives, opt => opt.Ignore());


            CreateMap<BaseDetailForm, BaseDetail>();

            #endregion


            #region Detail

            CreateMap<DetailEntity, Detail>()
                .ForMember(dest => dest.Self, opt => opt.MapFrom(src =>
                    Link.To(nameof(Controllers.DetailsController.GetDetailByIdAsync), new { detailId = src.Id })))

                .ForMember(dest => dest.Field, opt => opt.MapFrom(src =>
                    JsonConvert.DeserializeObject(src.Field)))

                .ForMember(dest => dest.Asset, opt => opt.MapFrom(src =>
                    Link.To(nameof(Controllers.AssetsController.GetAssetByIdAsync),
                        new { assetId = src.AssetId })))

                .ForMember(dest => dest.BaseDetail, opt => opt.MapFrom(src =>
                    Link.To(nameof(Controllers.BaseDetailsController.GetBaseDetailByIdAsync),
                        new { baseDetailId = src.Id })))

                .ForMember(dest => dest.Label, opt => opt.MapFrom(src => 
                    src.BaseDetail == null ? null : src.BaseDetail.Label))

                .ForMember(dest => dest.FormSpecs, opt => opt.MapFrom(src =>
                    Link.ToCollection(
                        nameof(Controllers.DocsController.GetDocsByResourceId),
                        new { resourceType = Controllers.DocsController.Details, resourceId = src.Id })));

            CreateMap<Detail, DetailEntity>()
                .ForMember(dest => dest.Field, opt => opt.MapFrom(src =>
                    JsonConvert.SerializeObject(src.Field)))

                .ForMember(dest => dest.Asset, opt => opt.Ignore())
                .ForMember(dest => dest.BaseDetail, opt => opt.Ignore());

            CreateMap<DetailForm, Detail>();

            #endregion
        }
    }
}
