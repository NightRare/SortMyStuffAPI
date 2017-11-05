using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.Kestrel.Internal.System.Collections.Sequences;
using Microsoft.EntityFrameworkCore;
using SortMyStuffAPI.Models.Entities;
using SortMyStuffAPI.Services;
using StackExchange.Redis;

namespace SortMyStuffAPI.Utils
{
    public static class TestDataRepository
    {

        public static void LoadAllIntoContext(SortMyStuffContext context)
        {
            LoadAssetTrees(context);

            context.SaveChanges();
        }

        public static void LoadAssetTrees(SortMyStuffContext context)
        {
            var root = new AssetTreeEntity
            {
                Id = ApiStrings.ROOT_ASSET_ID,
                Contents = new List<AssetTreeEntity>()
            };

            var asset_1 = new AssetTreeEntity
            {
                Id = "1",
                Contents = new List<AssetTreeEntity>()
            };
            root.Contents.Add(asset_1);

            var asset_2 = new AssetTreeEntity
            {
                Id = "2",
                Contents = new List<AssetTreeEntity>()
            };
            root.Contents.Add(asset_2);

            var asset_3 = new AssetTreeEntity
            {
                Id = "3",
                Contents = new List<AssetTreeEntity>()
            };
            root.Contents.Add(asset_3);

            var asset_1_1 = new AssetTreeEntity
            {
                Id = "4",
                Contents = new List<AssetTreeEntity>()
            };
            asset_1.Contents.Add(asset_1_1);

            var asset_1_2 = new AssetTreeEntity
            {
                Id = "5",
                Contents = new List<AssetTreeEntity>()
            };
            asset_1.Contents.Add(asset_1_2);

            var asset_2_1 = new AssetTreeEntity
            {
                Id = "6",
                Contents = new List<AssetTreeEntity>()
            };
            asset_2.Contents.Add(asset_2_1);

            var asset_2_2 = new AssetTreeEntity
            {
                Id = "7",
                Contents = new List<AssetTreeEntity>()
            };
            asset_2.Contents.Add(asset_2_2);

            var asset_2_2_1 = new AssetTreeEntity
            {
                Id = "8",
                Contents = new List<AssetTreeEntity>()
            };
            asset_2_2.Contents.Add(asset_2_2_1);

            context.AssetTrees.Add(root);
            context.AssetTrees.Add(asset_1);
            context.AssetTrees.Add(asset_2);
            context.AssetTrees.Add(asset_3);
            context.AssetTrees.Add(asset_1_1);
            context.AssetTrees.Add(asset_1_2);
            context.AssetTrees.Add(asset_2_1);
            context.AssetTrees.Add(asset_2_2);
            context.AssetTrees.Add(asset_2_2_1);
        }

        public static void LoadAssets(SortMyStuffContext context)
        {

        }
    }
}
