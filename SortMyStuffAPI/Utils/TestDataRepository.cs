using System;
using System.Collections.Generic;
using SortMyStuffAPI.Services;
using SortMyStuffAPI.Models;
using SortMyStuffAPI.Infrastructure;

namespace SortMyStuffAPI.Utils
{
    public static class TestDataRepository
    {

        public static void LoadAllIntoContext(SortMyStuffContext context)
        {
            LoadAssetTrees(context);
            LoadAssets(context);

            context.SaveChanges();
        }

        public static void LoadAssetTrees(SortMyStuffContext context)
        {
            var root = new AssetTreeEntity
            {
                Id = ApiStrings.ROOT_ASSET_ID,
                Name = "Assets",
                Contents = new List<AssetTreeEntity>()
            };

            var asset_1 = new AssetTreeEntity
            {
                Id = "1",
                Name = "asset_1",
                Contents = new List<AssetTreeEntity>()
            };
            root.Contents.Add(asset_1);

            var asset_2 = new AssetTreeEntity
            {
                Id = "2",
                Name = "asset_2",
                Contents = new List<AssetTreeEntity>()
            };
            root.Contents.Add(asset_2);

            var asset_1_1 = new AssetTreeEntity
            {
                Id = "1_1",
                Name = "asset_1_1",
                Contents = new List<AssetTreeEntity>()
            };
            asset_1.Contents.Add(asset_1_1);

            var asset_1_2 = new AssetTreeEntity
            {
                Id = "1_2",
                Name = "asset_1_2",
                Contents = new List<AssetTreeEntity>()
            };
            asset_1.Contents.Add(asset_1_2);

            var asset_2_1 = new AssetTreeEntity
            {
                Id = "2_1",
                Name = "asset_2_1",
                Contents = new List<AssetTreeEntity>()
            };
            asset_2.Contents.Add(asset_2_1);

            var asset_1_1_1 = new AssetTreeEntity
            {
                Id = "1_1_1",
                Name = "asset_1_1_1",
                Contents = new List<AssetTreeEntity>()
            };
            asset_1_1.Contents.Add(asset_1_1_1);

            context.AssetTrees.Add(root);
            context.AssetTrees.Add(asset_1);
            context.AssetTrees.Add(asset_2);
            context.AssetTrees.Add(asset_1_1);
            context.AssetTrees.Add(asset_1_2);
            context.AssetTrees.Add(asset_2_1);
            context.AssetTrees.Add(asset_1_1_1);
        }

        public static void LoadAssets(SortMyStuffContext context)
        {
            var timestamp = DateTimeOffset.UtcNow;
            var root = new AssetEntity
            {
                Id = ApiStrings.ROOT_ASSET_ID,
                Name = "Assets",
                ContainerId = null,
                Category = null,
                CreateTimestamp = timestamp,
                ModifyTimestamp = timestamp
            };

            var asset_1 = new AssetEntity
            {
                Id = "1",
                Name = "asset_1",
                ContainerId = root.Id,
                Category = "Place",
                CreateTimestamp = timestamp,
                ModifyTimestamp = timestamp
            };

            var asset_2 = new AssetEntity
            {
                Id = "2",
                Name = "asset_2",
                ContainerId = root.Id,
                Category = "Place",
                CreateTimestamp = timestamp,
                ModifyTimestamp = timestamp
            };

            var asset_1_1 = new AssetEntity
            {
                Id = "1_1",
                Name = "asset_1_1",
                ContainerId = asset_1.Id,
                Category = "Miscellaneous",
                CreateTimestamp = timestamp,
                ModifyTimestamp = timestamp
            };

            var asset_1_2 = new AssetEntity
            {
                Id = "1_2",
                Name = "asset_1_2",
                ContainerId = asset_1.Id,
                Category = "Miscellaneous",
                CreateTimestamp = timestamp,
                ModifyTimestamp = timestamp
            };

            var asset_2_1 = new AssetEntity
            {
                Id = "2_1",
                Name = "asset_2_1",
                ContainerId = asset_2.Id,
                Category = "Miscellaneous",
                CreateTimestamp = timestamp,
                ModifyTimestamp = timestamp
            };

            var asset_1_1_1 = new AssetEntity
            {
                Id = "1_1_1",
                Name = "asset_1_1_1",
                ContainerId = asset_1_1.Id,
                Category = "Book",
                CreateTimestamp = timestamp,
                ModifyTimestamp = timestamp
            };

            context.Assets.Add(root);
            context.Assets.Add(asset_1);
            context.Assets.Add(asset_2);
            context.Assets.Add(asset_1_1);
            context.Assets.Add(asset_1_2);
            context.Assets.Add(asset_2_1);
            context.Assets.Add(asset_1_1_1);
        }
    }
}
