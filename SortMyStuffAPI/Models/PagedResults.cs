using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SortMyStuffAPI.Models
{
    public class PagedResults<T>
    {
        public IEnumerable<T> PagedItems { get; set; }

        public int TotalSize { get; set; }
    }
}
