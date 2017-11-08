﻿using AutoMapper;
using SortMyStuffAPI.Models;
using SortMyStuffAPI.Utils;

namespace SortMyStuffAPI.Infrastructure
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<AssetTreeEntity, AssetTree>()
                .ForMember(dest => dest.Self, opt => opt.MapFrom(src =>
                    Link.To(nameof(Controllers.AssetTreesController.GetAssetTreeByIdAsync), new { assetTreeId = src.Id })))
                .MaxDepth(1000)
                .PreserveReferences();

            CreateMap<AssetEntity, Asset>()
                .ForMember(dest => dest.Self, opt => opt.MapFrom(src =>
                    Link.To(nameof(Controllers.AssetsController.GetAssetByIdAsync), new { assetId = src.Id })))

                .ForMember(dest => dest.Path, opt => opt.MapFrom(src =>
                    Link.ToCollection(nameof(Controllers.AssetsController.GetAssetPathByIDAsync), new { assetId = src.Id })))

                .ForMember(dest => dest.Thumbnail, opt => opt.MapFrom(src =>
                    Link.To(nameof(Controllers.ThumbnailsController.GetThumbnailById), new { assetId = src.Id })))

                .ForMember(dest => dest.ContentAssets, opt => opt.MapFrom(src =>
                    Link.ToCollection(
                        nameof(Controllers.AssetsController.GetAssetsAsync),
                        new { search = $"{ nameof(Asset.ContainerId).ToCamelCase() } { ApiStrings.PARAMETER_OP_EQUAL } { src.Id }" })))

                .ForMember(dest => dest.FormSpecs, opt => opt.MapFrom(src =>
                    Link.ToCollection(
                        nameof(Controllers.DocsController.GetDocsByResourceId),
                        new { resourceType = nameof(Asset).ToCamelCase(), resourceId = src.Id })));
        }
    }
}
