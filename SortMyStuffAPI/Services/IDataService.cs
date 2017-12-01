using System.Threading;
using System.Threading.Tasks;
using SortMyStuffAPI.Models;
using System.Reflection;
using SortMyStuffAPI.Infrastructure;
using SortMyStuffAPI.Exceptions;
using System.Collections.Generic;
using System;

namespace SortMyStuffAPI.Services
{
    public interface IDataService<T, TEntity>
        where T : EntityResource
        where TEntity : class, IEntity
    {
        Task<T> GetResourceAsync(
            string userId,
            string resourceId,
            CancellationToken ct);

        Task<PagedResults<T>> GetResouceCollectionAsync(
            string userId,
            CancellationToken ct,
            PagingOptions pagingOptions = null,
            SortOptions<T, TEntity> sortOptions = null,
            SearchOptions<T, TEntity> searchOptions = null);

        Task<(bool Succeeded, string Error)> AddResourceAsync(
            string userId,
            T resource,
            CancellationToken ct);

        Task<(bool Succeeded, string Error)> AddResourceCollectionAsync(
            string userId,
            ICollection<T> resources,
            CancellationToken ct);

        Task<(bool Succeeded, string Error)> UpdateResourceAsync(
            string userId,
            T resource,
            CancellationToken ct);

        Task<(bool Succeeded, string Error)> DeleteResourceAsync(
            string userId,
            string resourceId,
            bool delDependents);

        /// <summary>
        /// Checks if the value of the resource's specified property is unique 
        /// in a certain scope.
        /// 
        /// For instance, (category, Scop.User, category.GetType().GetProperty("Name"))
        /// this will check whether the value of category.Name is unique.
        /// 
        /// Make sure the resource given and the corresponding entity type has 
        /// a property named as "{Scope}Id" (e.g. "UserId", "CategoryId") which 
        /// serves as a foreign key referring to the {Scope} entity (e.g. the Id of UserEntity, CategoryEntity).
        /// </summary>
        /// 
        /// <param name="userId">
        /// The id of the user who requests the query.
        /// </param>
        /// 
        /// <param name="property">
        /// The properties whose values will be checked.
        /// </param>
        /// 
        /// <param name="resource">
        /// The resource to be checked.
        /// </param>
        /// 
        /// <param name="scope">
        /// The scope to be checked in.
        /// </param>
        /// 
        /// <param name="ct">
        /// The cancellation token.
        /// </param>
        /// 
        /// <returns>
        /// true if the value is unique in the specified scope;
        /// </returns>
        /// 
        /// <exception cref="ApiException">
        /// If the given scope is not found;
        /// or if the resource does not have corresponding scope Id property;
        /// or if the resource or corresponding entity does not contain the given 
        /// property(ies)
        /// </exception>
        Task<bool> CheckScopedUniquenessAsync(
            string userId,
            PropertyInfo property,
            T resource,
            Scope scope,
            CancellationToken ct);
    }
}
