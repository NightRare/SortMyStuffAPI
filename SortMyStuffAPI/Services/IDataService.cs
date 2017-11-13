using System.Threading;
using System.Threading.Tasks;
using SortMyStuffAPI.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;

namespace SortMyStuffAPI.Services
{
    public interface IDataService<T, TEntity> 
        where T : Resource 
        where TEntity : IEntity
    {
        Task<T> GetResourceAsync(
            string id, 
            CancellationToken ct, 
            string userId = null);

        Task<PagedResults<T>> GetResouceCollectionAsync(
            CancellationToken ct,
            PagingOptions pagingOptions = null,
            SortOptions<T, TEntity> sortOptions = null,
            SearchOptions<T, TEntity> searchOptions = null,
            string userId = null);
    }
}
