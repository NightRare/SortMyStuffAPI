using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SortMyStuffAPI.Services;
using SortMyStuffAPI.Models;
using SortMyStuffAPI.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace SortMyStuffAPI.Utils
{
    public static class TestDataFactory
    {
        public readonly static string DeveloperUid = Environment.GetEnvironmentVariable(ApiStrings.EnvDeveloperUid);
        public const string DeveloperRootAssetId = "rootassetid";

        public const string TestUserRootAssetId = "testrootassetid";
        public const string TestUserId = "testuserid";
        public const string TestUserEmail = "testuser@test.com";
        public const string TestUserPass = "Test1234.";

        public async static Task DeleteAllData(
            SortMyStuffContext db)
        {
            var prefix = "dbo.";

            var tables = new string[]
            {
                "Details",
                "BaseDetails",
                "Assets",
                "Categories",

                "OpenIddictTokens",
                "OpenIddictAuthorizations",
                "OpenIddictApplications",
                "OpenIddictScopes",

                "AspNetUserTokens",
                "AspNetUserClaims",
                "AspNetUserLogins",
                "AspNetUserRoles",
                "AspNetUsers",
                "AspNetRoleClaims",
                "AspNetRoles",
            };

            foreach (var s in tables)
            {
                var sql = $"DELETE FROM {prefix + s}";
                var result = await db.Database.ExecuteSqlCommandAsync(sql);
            }
            await db.SaveChangesAsync();
        }


        public static async Task LoadRolesAndUsers(
            RoleManager<UserRoleEntity> roleManager,
            UserManager<UserEntity> userManager)
        {
            if (roleManager != null)
            {
                await roleManager.CreateAsync(new UserRoleEntity(ApiStrings.RoleDeveloper));
            }

            if (userManager != null)
            {
                // developer user

                var developer = new UserEntity
                {
                    Id = DeveloperUid,
                    Email = Environment.GetEnvironmentVariable(ApiStrings.EnvDeveloperEmail),
                    UserName = "Developer",
                    CreateTimestamp = DateTimeOffset.UtcNow,
                    RootAssetId = DeveloperRootAssetId
                };

                await userManager.CreateAsync(
                    developer,
                    Environment.GetEnvironmentVariable(ApiStrings.EnvDeveloperPassword));

                await userManager.AddToRoleAsync(developer, ApiStrings.RoleDeveloper);
                await userManager.UpdateAsync(developer);

                // test user 

                var testuser = new UserEntity
                {
                    Id = TestUserId,
                    Email = TestUserEmail,
                    UserName = "TestUser",
                    CreateTimestamp = DateTimeOffset.UtcNow,
                    RootAssetId = TestUserRootAssetId
                };

                await userManager.CreateAsync(
                    testuser, TestUserPass);
            }
        }

        public async static Task LoadData(
            SortMyStuffContext context,
            UserManager<UserEntity> userManager)
        {

            #region Load Categories

            var categories = new List<CategoryEntity>
            {
                new CategoryEntity
                {
                    Id = "c1",
                    Name = "Place",
                },

                new CategoryEntity
                {
                    Id = "c2",
                    Name = "Book",
                },

                new CategoryEntity
                {
                    Id = "c3",
                    Name = "Miscellaneous",
                }
            };

            foreach (var c in categories)
            {
                c.BaseDetails = new List<BaseDetailEntity>();
                c.CategorisedAssets = new List<AssetEntity>();
                c.UserId = DeveloperUid;
            }

            categories.Add(new CategoryEntity
            {
                Id = "c1_t",
                Name = "Place",
                UserId = TestUserId,
            });

            context.AddRange(categories);
            await context.SaveChangesAsync();

            #endregion

            #region Load Assets

            var timestamp = DateTimeOffset.UtcNow;

            var root = new AssetEntity
            {
                Id = DeveloperRootAssetId,
                Name = "Assets",
                ContainerId = ApiStrings.RootAssetToken,
                CategoryId = null,
                UserId = DeveloperUid
            };

            var asset_1 = new AssetEntity
            {
                Id = "a1",
                Name = "asset_1",
                ContainerId = root.Id,
                CategoryId = categories[0].Id,
                Category = categories[0]
            };

            var asset_2 = new AssetEntity
            {
                Id = "a2",
                Name = "asset_2",
                ContainerId = root.Id,
                CategoryId = categories[0].Id,
                Category = categories[0]
            };

            var asset_1_1 = new AssetEntity
            {
                Id = "a1_1",
                Name = "asset_1_1",
                ContainerId = asset_1.Id,
                CategoryId = categories[2].Id,
                Category = categories[2]
            };

            var asset_1_2 = new AssetEntity
            {
                Id = "a1_2",
                Name = "asset_1_2",
                ContainerId = asset_1.Id,
                CategoryId = categories[2].Id,
                Category = categories[2]
            };

            var asset_2_1 = new AssetEntity
            {
                Id = "a2_1",
                Name = "asset_2_1",
                ContainerId = asset_2.Id,
                CategoryId = categories[2].Id,
                Category = categories[2]
            };

            var asset_1_1_1 = new AssetEntity
            {
                Id = "a1_1_1",
                Name = "asset_1_1_1",
                ContainerId = asset_1_1.Id,
                CategoryId = categories[1].Id,
                Category = categories[1]
            };

            var devleoperAssets = new List<AssetEntity>()
            {
                root, asset_1, asset_2, asset_1_1, asset_1_2, asset_2_1, asset_1_1_1
            };

            foreach (var a in devleoperAssets)
            {
                a.CreateTimestamp = timestamp;
                a.ModifyTimestamp = timestamp;
                a.UserId = DeveloperUid;
            }

            context.AddRange(devleoperAssets);
            await context.SaveChangesAsync();

            //***************************************

            var root_t = new AssetEntity
            {
                Id = TestUserRootAssetId,
                Name = "Assets",
                ContainerId = ApiStrings.RootAssetToken,
                CategoryId = null,
                Category = null
            };

            var asset_1_t = new AssetEntity
            {
                Id = "a1_t",
                Name = "asset_1_t",
                ContainerId = root_t.Id,
                CategoryId = categories[3].Id,
                Category = categories[3]
            };

            var asset_2_t = new AssetEntity
            {
                Id = "a2_t",
                Name = "asset_2_t",
                ContainerId = root_t.Id,
                CategoryId = categories[3].Id,
                Category = categories[3]
            };

            var testuserAssets = new List<AssetEntity>()
            {
                root_t, asset_1_t, asset_2_t
            };

            foreach (var a in testuserAssets)
            {
                a.CreateTimestamp = timestamp;
                a.ModifyTimestamp = timestamp;
                a.UserId = TestUserId;
            }

            context.AddRange(testuserAssets);
            await context.SaveChangesAsync();

            #endregion
        }
    }
}
