using SortMyStuffAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SortMyStuffAPI.Services
{
    public interface IBaseDetailDataService 
        : IDataService<BaseDetail, BaseDetailEntity>
    {
        Task<IEnumerable<BaseDetail>> GetBaseDetailsByCategoryIdAsync(
            string userId,
            string categoryId,
            CancellationToken ct);
    }
}
