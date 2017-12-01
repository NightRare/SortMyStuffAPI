using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SortMyStuffAPI.Exceptions;
using SortMyStuffAPI.Infrastructure;
using SortMyStuffAPI.Models;
using SortMyStuffAPI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SortMyStuffAPI.Services
{
    public abstract class DefaultDataService<T, TEntity>
        where T : EntityResource
        where TEntity : class, IEntity
    {
        protected readonly SortMyStuffContext DbContext;
        protected readonly ApiConfigs ApiConfigs;

        protected DefaultDataService(
            SortMyStuffContext dbContext,
            IOptions<ApiConfigs> apiConfigs)
        {
            DbContext = dbContext;
            ApiConfigs = apiConfigs.Value;
        }

        protected virtual async Task<PagedResults<T>> GetOneTypeResourcesAsync(
            string userId,
            DbSet<TEntity> dbSet,
            CancellationToken ct,
            PagingOptions pagingOptions = null,
            SortOptions<T, TEntity> sortOptions = null,
            SearchOptions<T, TEntity> searchOptions = null)
        {
            IQueryable<TEntity> query = GetUserRepository(userId, dbSet);

            if (searchOptions != null)
            {
                query = new SearchOptionsProcessor<T, TEntity>(searchOptions)
                    .Apply(query);
            }

            if (sortOptions != null)
            {
                query = new SortOptionsProcessor<T, TEntity>(sortOptions)
                    .Apply(query);
            }

            IEnumerable<T> resources = await Task.Run(
                () => query.ProjectTo<T>().ToArray(),
                ct);

            var totalSize = resources.Count();

            if (pagingOptions != null)
            {
                var pagedResources = resources
                    .Skip(pagingOptions.Offset.Value)
                    .Take(pagingOptions.PageSize.Value);
                resources = pagedResources;
            }

            return new PagedResults<T>
            {
                PagedItems = resources,
                TotalSize = totalSize
            };
        }

        protected virtual async Task<T> GetOneResourceAsync(
            IQueryable<TEntity> dbSet,
            string userId,
            string id,
            CancellationToken ct)
        {
            var repo = GetUserRepository(userId, dbSet);
            var entity = await Task.Run(() => repo.SingleOrDefault(a => a.Id == id), ct);

            return entity == null ? default(T) : Mapper.Map<TEntity, T>(entity);
        }

        protected virtual async Task<(bool Succeeded, string Error)> AddOneResourceAsync(
            string userId,
            T resource,
            DbSet<TEntity> dbSet,
            CancellationToken ct)
        {
            if (userId == null)
            {
                return (false, "The userId cannot be null.");
            }

            return await Task.Run(() =>
            {
                var repo = GetUserRepository(userId, dbSet);

                var entity = Mapper.Map<T, TEntity>(resource);
                entity.UserId = userId;

                if (repo.Any(e => e.Id == entity.Id))
                    return (false, $"{resource.GetType().Name} already exists");

                try
                {
                    dbSet.Add(entity);
                    DbContext.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    return (false, ex.Message);
                }

                return (true, null);
            }, ct);
        }

        protected virtual async Task<(bool Succeeded, string Error)> AddResourceRangeAsync(
            string userId,
            ICollection<T> resources,
            DbSet<TEntity> dbSet,
            CancellationToken ct)
        {
            if (userId == null)
            {
                return (false, "The userId cannot be null.");
            }

            return await Task.Run(() =>
            {
                var repo = GetUserRepository(userId, dbSet);

                var entities = resources.AsQueryable()
                    .ProjectTo<TEntity>();

                foreach (var entity in entities)
                {
                    entity.UserId = userId;
                };

                try
                {
                    dbSet.AddRange(entities);
                    DbContext.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    return (false, ex.Message);
                }

                return (true, null);
            }, ct);
        }

        protected virtual async Task<(bool Succeeded, string Error)> UpdateOneResourceAsync(
            string userId,
            T resource,
            DbSet<TEntity> dbSet,
            CancellationToken ct)
        {
            var repo = GetUserRepository(userId, dbSet);
            return await Task.Run(() =>
            {
                var entity = repo.SingleOrDefault(a => a.Id == resource.Id);
                if (entity == null)
                    return (false, $"{resource.GetType().Name} not found");

                var allMutableProperties = entity
                    .GetType()
                    .GetProperties()
                    .Where(p => p.GetCustomAttributes<MutableAttribute>().Any());

                var updateSource = Mapper.Map<T, TEntity>(resource);

                foreach (var prop in allMutableProperties)
                {
                    var updatingValue = prop.GetValue(updateSource);
                    if (updatingValue != null)
                        prop.SetValue(entity, updatingValue);
                }

                try
                {
                    DbContext.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    return (false, ex.Message);
                }
                return (true, null);
            }, ct);
        }

        protected virtual async Task<(bool Succeeded, string Error)> DeleteOneResourceAsync(
            string resourceId,
            IQueryable<TEntity> repo)
        {
            return await Task.Run(() =>
            {
                var delEntity = repo.SingleOrDefault(a => a.Id == resourceId) ??
                    throw new KeyNotFoundException();

                DbContext.Remove<TEntity>(delEntity);

                try
                {
                    DbContext.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    return (false, ex.Message);
                }

                return (true, null);
            });
        }

        protected virtual bool CheckResourceScopedUniqueness(
            string userId,
            PropertyInfo property,
            T resource,
            Scope scope,
            DbSet<TEntity> dbSet)
        {
            IQueryable<TEntity> query = dbSet;

            // Trying to build expressions:
            // query = query.Where(e => e.ScopeProperty == ScopeId)
            // query.Any(e => e.Property == resource.Property)

            Expression scopeIdExp;
            PropertyInfo scopeProperty;
            switch (scope)
            {
                case Scope.User:
                    {
                        // Get the value of resource.UserId (i.e. ScopeId)
                        scopeIdExp = Expression.Constant(resource.UserId ?? userId) ??
                            throw new ApiException(
                                "Unable to get the value of userId");

                        // ?.UserId (i.e. ?.ScopeProperty)
                        scopeProperty = ExpressionHelper.GetPropertyInfo<TEntity>("UserId") ??
                            throw new ApiException(
                                "Unable to locate the scope. 'UserId' property not found.");
                        break;
                    }
                case Scope.Category:
                    {
                        // ?.CategoryId (i.e. ?.ScopeProperty)
                        scopeProperty = ExpressionHelper.GetPropertyInfo<TEntity>("CategoryId") ??
                            throw new ApiException(
                                "Unable to locate the scope. 'CategoryId' property not found.");

                        var categoryIdProp = resource.GetType().GetProperty("CategoryId") ??
                            throw new ApiException(
                                "Unable to locate the scope. 'CategoryId' property not found.");

                        // Get the value of resource.CategoryId (i.e. ScopeId)
                        scopeIdExp = Expression.Constant(categoryIdProp.GetValue(resource)) ??
                            throw new ApiException(
                                "Unable to get the value of categoryId");

                        break;
                    }
                default:
                    throw new ApiException("Unexpected scope");
            }

            // stage 1: query = dbSet.Where(e => e.UserId == resource.UserId)

            // e
            var entityObj = Expression.Parameter(typeof(TEntity), "e");

            // e.ScopeProperty
            var entityScopeProperty = ExpressionHelper.GetPropertyExpression(
                entityObj, scopeProperty);

            // e.ScopeProperty == ScopeId
            var comparisonScopeId = Expression.Equal(
                entityScopeProperty, scopeIdExp);

            // e => e.UserId == resource.UserId
            var lambdaComparisonScopeId = ExpressionHelper
                .GetLambda<TEntity, bool>(entityObj, comparisonScopeId);

            // query = query.Where...
            query = ExpressionHelper.CallWhere(query, lambdaComparisonScopeId);

            // then check uniqueness
            // stage 2: query.Any(e => e.Property == resource.Property)

            // e.Property
            var entityProperty = ExpressionHelper.GetPropertyExpression(
                entityObj, typeof(TEntity).GetProperty(property.Name));

            // the value of resource.Property
            var resourcePropertyValue = resource.GetType()
                .GetProperty(property.Name)?.GetValue(resource) ??
                throw new ApiException(
                    $"Check scoped uniqueness failed. " +
                    $"The value of {property.Name} should not be null.");
            var resourceProperty = Expression.Constant(resourcePropertyValue);

            // e.Property == resource.Property
            var comparisonProperty = Expression.Equal(
                entityProperty, resourceProperty);

            // e => e.Property == resource.Property
            var lambdaComparisonProperty = ExpressionHelper.GetLambda<TEntity, bool>(
                entityObj, comparisonProperty);

            // query.Any(...)
            return !ExpressionHelper.CallAny(query, lambdaComparisonProperty);
        }

        protected static IQueryable<REntity> GetUserRepository<REntity>(
            string userId,
            IQueryable<REntity> dbSet)
            where REntity : class, IEntity
        {
            if (userId == null)
                throw new ArgumentException("The userId cannot be null.");

            if (ServicesAuthHelper.IsDeveloper(userId))
                return dbSet;

            return dbSet.Where(e => e.UserId == userId);
        }

    }
}
