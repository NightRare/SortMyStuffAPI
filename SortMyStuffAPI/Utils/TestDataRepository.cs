using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SortMyStuffAPI.Services;
using SortMyStuffAPI.Models;
using SortMyStuffAPI.Infrastructure;

namespace SortMyStuffAPI.Utils
{
    public static class TestDataRepository
    {
        private const string RootAssetId = "rootassetid";

        public static async Task LoadRolesAndUsers(
            RoleManager<UserRoleEntity> roleManager,
            UserManager<UserEntity> userManager)
        {
            if (roleManager != null)
            {
                await LoadUserRoles(roleManager);
            }

            if (userManager != null)
            {
                await LoadUsers(userManager);
            }
        }

        public static void LoadData(
            SortMyStuffContext context)
        {
            var categories = LoadCategories(context);
            var assets = LoadAssets(context, categories);

            context.SaveChanges();
        }

        private static async Task LoadUserRoles(
            RoleManager<UserRoleEntity> roleManager)
        {
            await roleManager.CreateAsync(new UserRoleEntity("Admin"));
        }

        private async static Task LoadUsers(
            UserManager<UserEntity> userManager)
        {
            var user = new UserEntity
            {
                Id = Guid.NewGuid().ToString(),
                Email = Environment.GetEnvironmentVariable(ApiStrings.EnvFirebaseAuthEmail),
                UserName = "Admin",
                CreateTimestamp = DateTimeOffset.UtcNow
            };

            await userManager.CreateAsync(
                user, 
                Environment.GetEnvironmentVariable(ApiStrings.EnvFirebaseAuthPassword));

            await userManager.AddToRoleAsync(user, "Admin");
            await userManager.UpdateAsync(user);
        }

        private static IList<CategoryEntity> LoadCategories(SortMyStuffContext context)
        {
            var categories = new List<CategoryEntity>();

            categories.Add(new CategoryEntity
            {
                Id = "c1",
                Name = "Place",
                BaseDetails = new List<BaseDetailEntity>(),
                CategorisedAssets = new List<AssetEntity>()
            });

            categories.Add(new CategoryEntity
            {
                Id = "c2",
                Name = "Book",
                BaseDetails = new List<BaseDetailEntity>(),
                CategorisedAssets = new List<AssetEntity>()
            });

            categories.Add(new CategoryEntity
            {
                Id = "c3",
                Name = "Miscellaneous",
                BaseDetails = new List<BaseDetailEntity>(),
                CategorisedAssets = new List<AssetEntity>()
            });

            context.AddRange(categories);

            return categories;
        }

        private static IList<AssetEntity> LoadAssets(SortMyStuffContext context, IList<CategoryEntity> categories)
        {
            var timestamp = DateTimeOffset.UtcNow;

            var root = new AssetEntity
            {
                Id = RootAssetId,
                Name = "Assets",
                ContainerId = null,
                CategoryId = null,
                CreateTimestamp = timestamp,
                ModifyTimestamp = timestamp,
                Category = null
            };

            var asset_1 = new AssetEntity
            {
                Id = "a1",
                Name = "asset_1",
                ContainerId = root.Id,
                CategoryId = categories[0].Id,
                CreateTimestamp = timestamp,
                ModifyTimestamp = timestamp,
                Category = categories[0]
            };

            var asset_2 = new AssetEntity
            {
                Id = "a2",
                Name = "asset_2",
                ContainerId = root.Id,
                CategoryId = categories[0].Id,
                CreateTimestamp = timestamp,
                ModifyTimestamp = timestamp,
                Category = categories[0]
            };

            var asset_1_1 = new AssetEntity
            {
                Id = "a1_1",
                Name = "asset_1_1",
                ContainerId = asset_1.Id,
                CategoryId = categories[2].Id,
                CreateTimestamp = timestamp,
                ModifyTimestamp = timestamp,
                Category = categories[2]
            };

            var asset_1_2 = new AssetEntity
            {
                Id = "a1_2",
                Name = "asset_1_2",
                ContainerId = asset_1.Id,
                CategoryId = categories[2].Id,
                CreateTimestamp = timestamp,
                ModifyTimestamp = timestamp,
                Category = categories[2]
            };

            var asset_2_1 = new AssetEntity
            {
                Id = "a2_1",
                Name = "asset_2_1",
                ContainerId = asset_2.Id,
                CategoryId = categories[2].Id,
                CreateTimestamp = timestamp,
                ModifyTimestamp = timestamp,
                Category = categories[2]
            };

            var asset_1_1_1 = new AssetEntity
            {
                Id = "a1_1_1",
                Name = "asset_1_1_1",
                ContainerId = asset_1_1.Id,
                CategoryId = categories[1].Id,
                CreateTimestamp = timestamp,
                ModifyTimestamp = timestamp,
                Category = categories[1]
            };

            var assets = new List<AssetEntity>()
            {
                root, asset_1, asset_2, asset_1_1, asset_1_2, asset_2_1, asset_1_1_1
            };

            context.AddRange(assets);

            return assets;
        }
    }
}
