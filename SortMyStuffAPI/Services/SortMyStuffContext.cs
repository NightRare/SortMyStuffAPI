using Microsoft.EntityFrameworkCore;
using SortMyStuffAPI.Models;

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
