using System.Threading;
using System.Threading.Tasks;
using SortMyStuffAPI.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using System.Data.Entity;

namespace SortMyStuffAPI.Services
{
    public interface IDataService<T, TEntity>
        where T : Resource
        where TEntity : class, IEntity
    {
        Task<T> GetResourceAsync(
            string userId,
            string id,
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
    }
}
