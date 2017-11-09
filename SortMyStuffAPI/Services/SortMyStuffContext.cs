using Microsoft.EntityFrameworkCore;
using SortMyStuffAPI.Models;

namespace SortMyStuffAPI.Services
{
    public class SortMyStuffContext : DbContext
    {
        public SortMyStuffContext(DbContextOptions opt) : base(opt)
        { }

        public DbSet<AssetEntity> Assets { get; set; }
    }
}
