﻿using System.Threading;
using System.Threading.Tasks;
using SortMyStuffAPI.Models;

namespace SortMyStuffAPI.Services
{
    public interface ICategoryDataService 
        : IDataService<Category, CategoryEntity>
    {
    }
}
