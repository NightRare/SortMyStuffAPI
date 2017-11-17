using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SortMyStuffAPI.Infrastructure;
using SortMyStuffAPI.Models;

namespace SortMyStuffAPI.Services
{
    public class DefaultDetailDataService
        : DefaultBaseDataService,
        IDetailDataService
    {
        public DefaultDetailDataService(
            SortMyStuffContext dbContext, 
            IOptions<ApiConfigs> apiConfigs) : 
            base(dbContext, apiConfigs)
        {
        }

        public Task<(bool Succeeded, string Error)> AddResourceAsync(
            string userId, 
            Detail resource, 
            CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CheckScopedUniquenessAsync(
            string userId, 
            PropertyInfo property, 
            Detail resource, 
            Scope scope, 
            CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<(bool Succeeded, string Error)> DeleteResourceAsync(
            string userId, 
            string resourceId,
            bool delDependents, 
            CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResults<Detail>> GetResouceCollectionAsync(
            string userId, 
            CancellationToken ct, 
            PagingOptions pagingOptions = null, 
            SortOptions<Detail, DetailEntity> sortOptions = null, 
            SearchOptions<Detail, DetailEntity> searchOptions = null)
        {
            throw new NotImplementedException();
        }

        public Task<Detail> GetResourceAsync(
            string userId,
            string resourceId,
            CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<(bool Succeeded, string Error)> UpdateResourceAsync(
            string userId,
            Detail resource,
            CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
