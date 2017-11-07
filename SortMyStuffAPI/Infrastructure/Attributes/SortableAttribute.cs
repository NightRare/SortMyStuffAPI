using System;

namespace SortMyStuffAPI.Infrastructure
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class SortableAttribute : Attribute
    {
        public bool Default { get; set; }
    }
}
