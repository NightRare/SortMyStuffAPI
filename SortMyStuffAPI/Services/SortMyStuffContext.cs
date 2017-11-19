using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SortMyStuffAPI.Models;

namespace SortMyStuffAPI.Services
{
    public class SortMyStuffContext : IdentityDbContext<UserEntity, UserRoleEntity, string>
    {
        public SortMyStuffContext(DbContextOptions opt) : base(opt)
        {
        }

        public DbSet<AssetEntity> Assets { get; set; }
        public DbSet<CategoryEntity> Categories { get; set; }
        public DbSet<BaseDetailEntity> BaseDetails { get; set; }
        public DbSet<DetailEntity> Details { get; set; }
        public DbSet<UserRootAssetEntity> UserRootAssetContracts { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            #region BaseDetail

            builder.Entity<BaseDetailEntity>()
                .HasIndex(bd => bd.CategoryId)
                .ForSqlServerIsClustered();

            builder.Entity<BaseDetailEntity>()
                .HasIndex(bd => bd.UserId);

            #endregion

            #region Category

            builder.Entity<CategoryEntity>()
                .HasIndex(c => c.UserId)
                .ForSqlServerIsClustered();

            builder.Entity<CategoryEntity>()
                .HasIndex(c => c.Name);

            #endregion

            #region UserRootAsset

            builder.Entity<UserRootAssetEntity>()
                .HasIndex(c => c.UserId)
                .IsUnique();

            builder.Entity<UserRootAssetEntity>()
                .HasIndex(c => c.RootAssetId)
                .IsUnique();

            #endregion

            #region Asset

            builder.Entity<AssetEntity>()
                .HasIndex(a => a.CategoryId);

            builder.Entity<AssetEntity>()
                .HasIndex(a => a.ContainerId);

            builder.Entity<AssetEntity>()
                .HasIndex(a => a.UserId)
                .ForSqlServerIsClustered();

            #endregion

            #region Detail

            builder.Entity<DetailEntity>()
                .HasIndex(e => e.BaseId)
                .ForSqlServerIsClustered();

            builder.Entity<DetailEntity>()
                .HasIndex(e => e.UserId);

            #endregion

        }
    }
}
