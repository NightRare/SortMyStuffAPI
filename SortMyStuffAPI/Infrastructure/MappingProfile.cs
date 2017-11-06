using AutoMapper;
using SortMyStuffAPI.Models;

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
                .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom(src =>
                    Link.To(nameof(Controllers.ThumbnailsController.GetThumbnailById), new { assetId = src.Id })));
        }
    }
}
