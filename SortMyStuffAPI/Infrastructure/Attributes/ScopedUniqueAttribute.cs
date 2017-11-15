using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SortMyStuffAPI.Infrastructure
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ScopedUniqueAttribute : Attribute
    {
        public Scope Scope { get; set; }
    }

    public enum Scope
    {
        User = 0,
        Category = 1
    }
}
