using System;

namespace SortMyStuffAPI.Infrastructure
{
    /// <summary>
    /// This attribute applies to the properties of Entites.
    /// Only properties with this attribute is able to be changed after 
    /// the record is created. This attribute will be checked during 
    /// data layer updating process.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MutableAttribute: Attribute
    {
    }
}
