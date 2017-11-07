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

                .ForMember(dest => dest.Container, opt => opt.MapFrom(src =>
                    Link.To(nameof(Controllers.AssetsController.GetAssetByIdAsync), new { assetId = src.ContainerId })))

                .ForMember(dest => dest.Thumbnail, opt => opt.MapFrom(src =>
                    Link.To(nameof(Controllers.ThumbnailsController.GetThumbnailById), new { assetId = src.Id })))
                
                .ForMember(dest => dest.UpdateAsset, opt => opt.MapFrom(src =>
                    FormMetadata.FromModel(
                        new UpdateAssetForm(), 
                        Link.ToForm(
                            nameof(Controllers.AssetsController.UpdateAssetByIdAsync),
                            new { assetId = src.Id },
                            ApiStrings.PUT_METHOD,
                            Form.EditRelation))));
        }
    }
}
