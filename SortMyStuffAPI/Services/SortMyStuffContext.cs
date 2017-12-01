using System;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SortMyStuffAPI.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SortMyStuffAPI.Infrastructure;

namespace SortMyStuffAPI.Services
{
    public class SortMyStuffContext : IdentityDbContext<UserEntity, UserRoleEntity, string>
    {
        private readonly IDataChangeHanlder _dataChangeHanlder;
        private readonly object _lock = new object();

        public SortMyStuffContext(
            DbContextOptions opt,
            IDataChangeHanlder dataChangeHanlder) : base(opt)
        {
            _dataChangeHanlder = dataChangeHanlder;
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

        public override int SaveChanges()
        {
            if(!_dataChangeHanlder.AnyRegistration())
            {
                return base.SaveChanges();
            }

            lock(_lock)
            {
                try
                {
                    var recordedChanges = RecordChanges();
                    var result = base.SaveChanges();
                    new Task(() => _dataChangeHanlder.OnDataChanged(recordedChanges)).Start();
                    return result;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    throw ex;
                }
                catch (DbUpdateException ex)
                {
                    throw ex;
                }

            }
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            if (!_dataChangeHanlder.AnyRegistration())
            {
                return base.SaveChanges(acceptAllChangesOnSuccess);
            }

            lock (_lock)
            {
                try
                {
                    var recordedChanges = RecordChanges();
                    var result = base.SaveChanges(acceptAllChangesOnSuccess);
                    new Task(() => _dataChangeHanlder.OnDataChanged(recordedChanges)).Start();
                    return result;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    throw ex;
                }
                catch (DbUpdateException ex)
                {
                    throw ex;
                }
            }
        }

        public override async Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Task.Run(() => SaveChanges(acceptAllChangesOnSuccess), cancellationToken);
        }

        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Task.Run(() => SaveChanges(), cancellationToken);
        }

        private DataChangeEventArgs[] RecordChanges()
        {
            List<DataChangeEventArgs> changes = new List<DataChangeEventArgs>();
            foreach (var entry in ChangeTracker.Entries().ToArray())
            {
                var resource = (entry.Entity as IEntity).ToResource();
                if (resource == null) continue;
                changes.Add(new DataChangeEventArgs(
                    resource, entry.State));

            }
            return changes.ToArray();
        }
    }
}
