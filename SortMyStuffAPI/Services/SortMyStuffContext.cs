using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SortMyStuffAPI.Models;
using SortMyStuffAPI.Models.Entities;

namespace SortMyStuffAPI.Services
{
    public class SortMyStuffContext : DbContext
    {
        public SortMyStuffContext(DbContextOptions opt) : base(opt)
        { }

        public DbSet<AssetTreeEntity> AssetTrees { get; set; }

        public DbSet<AssetEntity> Assets { get; set; }
    }
}
