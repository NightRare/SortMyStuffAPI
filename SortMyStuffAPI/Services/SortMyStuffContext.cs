using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SortMyStuffAPI.Models;

namespace SortMyStuffAPI.Services
{
    public class SortMyStuffContext : IdentityDbContext<UserEntity, UserRoleEntity, string>
    {
        public SortMyStuffContext(DbContextOptions opt) : base(opt)
        { }

        public DbSet<AssetEntity> Assets { get; set; }
        public DbSet<CategoryEntity> Categories { get; set; }
        public DbSet<BaseDetailEntity> BaseDetails { get; set; }
        public DbSet<DetailEntity> Details { get; set; }
        public DbSet<UserRootAssetEntity> UserRootAssetContracts { get; set; }
    }
}
