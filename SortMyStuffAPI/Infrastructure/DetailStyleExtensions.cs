using System;
using System.Collections.Generic;
using SortMyStuffAPI.Models;
using Newtonsoft.Json;

namespace SortMyStuffAPI.Infrastructure
{
    public static class DetailStyleExtensions
    {
        private static IReadOnlyDictionary<DetailStyle, Type> TypeMapping = new Dictionary<DetailStyle, Type>()
        {
            [DetailStyle.Text] = typeof(string),
            [DetailStyle.Date] = typeof(DateTimeOffset)
        };

        public static Type GetFieldType(this DetailStyle style)
        {
            if (TypeMapping.TryGetValue(style, out var fieldType))
            {
                return fieldType;
            }

            return null;
        }
    }
}
