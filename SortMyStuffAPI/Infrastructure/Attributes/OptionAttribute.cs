using SortMyStuffAPI.Infrastructure.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SortMyStuffAPI.Infrastructure
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class OptionAttribute : Attribute
    {
        public IEnumerable<string> Options { get => GetEnumValues(); }

        private readonly Type _enumType;

        public OptionAttribute(Type enumType)
        {
            if (!enumType.IsEnum)
            {
                throw new ArgumentException(
                    "Option attribute must take in an Enum type.");
            }

            _enumType = enumType;
        }

        private IEnumerable<string> GetEnumValues()
        {
            var es = _enumType.GetFields();
            foreach (var field in _enumType.GetFields())
            {
                var ff = field.FieldType;
                if (!field.FieldType.Equals(_enumType) ||
                    field.GetCustomAttributes<HideOptionAttribute>().Any())
                {
                    continue;
                }
                yield return field.Name;
            }
        }
    }
}
