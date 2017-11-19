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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            #region BaseDetail

            // EF Code first will autmatically generate cluster on PK,
            // so have to set the PK here instead using [Key] attribute
            builder.Entity<BaseDetailEntity>()
                .HasKey(e => e.Id)
                .ForSqlServerIsClustered(false);

            builder.Entity<BaseDetailEntity>()
                .HasIndex(bd => bd.CategoryId)
                .ForSqlServerIsClustered(true);

            builder.Entity<BaseDetailEntity>()
                .HasIndex(bd => bd.UserId);

            // (BaseDetail => User) the foreign key is not required
            // set to false to exclude the cascade delete path on this relationship
            builder.Entity<BaseDetailEntity>()
                .HasOne(bd => bd.User)
                .WithMany(u => u.BaseDetails)
                .IsRequired(false);

            #endregion

            #region Category

            builder.Entity<CategoryEntity>()
                .HasKey(e => e.Id)
                .ForSqlServerIsClustered(false);

            builder.Entity<CategoryEntity>()
                .HasIndex(c => c.UserId)
                .ForSqlServerIsClustered(true);

            builder.Entity<CategoryEntity>()
                .HasIndex(c => c.Name);

            #endregion

            #region Asset

            builder.Entity<AssetEntity>()
                .HasKey(e => e.Id)
                .ForSqlServerIsClustered(false);

            builder.Entity<AssetEntity>()
                .HasIndex(a => a.CategoryId);

            builder.Entity<AssetEntity>()
                .HasIndex(a => a.ContainerId);

            builder.Entity<AssetEntity>()
                .HasIndex(a => a.UserId)
                .ForSqlServerIsClustered(true);

            builder.Entity<AssetEntity>()
                .HasOne(a => a.Category)
                .WithMany(c => c.CategorisedAssets)
                .IsRequired(false);

            #endregion

            #region Detail

            builder.Entity<DetailEntity>()
                .HasKey(e => e.Id)
                .ForSqlServerIsClustered(false);

            builder.Entity<DetailEntity>()
                .HasIndex(e => e.BaseId)
                .ForSqlServerIsClustered(true);

            builder.Entity<DetailEntity>()
                .HasIndex(e => e.UserId);

            builder.Entity<DetailEntity>()
                .HasOne(d => d.User)
                .WithMany(u => u.Details)
                .IsRequired(false);

            builder.Entity<DetailEntity>()
               .HasOne(d => d.Asset)
               .WithMany(a => a.Details)
               .IsRequired(false);

            #endregion
        }
    }
}
