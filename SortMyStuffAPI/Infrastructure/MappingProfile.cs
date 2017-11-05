using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using SortMyStuffAPI.Models;
using SortMyStuffAPI.Models.Entities;

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

        }
    }
}
