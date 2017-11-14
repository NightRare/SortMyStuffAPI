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
        public readonly static string DeveloperUid = Environment.GetEnvironmentVariable(ApiStrings.EnvDeveloperUid);
        public const string DeveloperRootAssetId = "rootassetid";

        public const string TestUserRootAssetId = "testrootassetid";
        public const string TestUserId = "testuserid";
        public const string TestUserEmail = "testuser@test.com";
        public const string TestUserPass = "Test1234.";

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

                await userManager.AddToRoleAsync(developer, ApiStrings.RoleDeveloper);
                await userManager.UpdateAsync(developer);
            }
        }

        public static void LoadData(
            SortMyStuffContext context,
            UserManager<UserEntity> userManager)
        {
            var developerUser = userManager
                .FindByIdAsync(DeveloperUid)
                .GetAwaiter()
                .GetResult();

            var testUser = userManager
                .FindByIdAsync(TestUserId)
                .GetAwaiter()
                .GetResult();

            #region Load Categories

            var categories = new List<CategoryEntity>();

            categories.Add(new CategoryEntity
            {
                Id = "c1",
                Name = "Place",
            });

            categories.Add(new CategoryEntity
            {
                Id = "c2",
                Name = "Book",
            });

            categories.Add(new CategoryEntity
            {
                Id = "c3",
                Name = "Miscellaneous",
            });

            foreach (var c in categories)
            {
                c.BaseDetails = new List<BaseDetailEntity>();
                c.CategorisedAssets = new List<AssetEntity>();
                c.UserId = DeveloperUid;
                c.User = developerUser;
            }

            categories.Add(new CategoryEntity
            {
                Id = "c1_t",
                Name = "Place",
                UserId = TestUserId,
                User = testUser
            });

            context.AddRange(categories);

            context.SaveChanges();

            #endregion

            #region Load Assets

            var timestamp = DateTimeOffset.UtcNow;

            var root = new AssetEntity
            {
                Id = DeveloperRootAssetId,
                Name = "Assets",
                ContainerId = null,
                CategoryId = null,
                Category = null
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

            var contract = new UserRootAssetEntity
            {
                Id = DeveloperUid + DeveloperRootAssetId,
                UserId = DeveloperUid,
                RootAssetId = DeveloperRootAssetId
            };

            context.UserRootAssetContracts.Add(contract);

            developerUser.RootAssetContractId = contract.Id;
            developerUser.RootAssetContract = contract;
            userManager.UpdateAsync(developerUser);


            //***************************************

            var root_t = new AssetEntity
            {
                Id = TestUserRootAssetId,
                Name = "Assets",
                ContainerId = null,
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

            var testcontract = new UserRootAssetEntity
            {
                Id = TestUserId + TestUserRootAssetId,
                UserId = TestUserId,
                RootAssetId = TestUserRootAssetId
            };

            context.UserRootAssetContracts.Add(testcontract);

            testUser.RootAssetContractId = testcontract.Id;
            testUser.RootAssetContract = testcontract;

            context.SaveChanges();
            userManager.UpdateAsync(testUser);

            #endregion
        }
    }
}
