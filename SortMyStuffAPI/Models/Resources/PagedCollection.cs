using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using SortMyStuffAPI.Models;
using System;

namespace SortMyStuffAPI.Models
{
    public class PagedCollection<T> : Collection<T>
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Offset { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? PageSize { get; set; }

        public int Size { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Link First { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Link Previous { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Link Next { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Link Last { get; set; }

        #region HELPERS

        public static PagedCollection<T> Create(
            Link self,
            T[] items,
            int size,
            PagingOptions pagingOptions)
            => new PagedCollection<T>
            {
                Self = self,
                Value = items,
                Size = size,
                Offset = pagingOptions.Offset,
                PageSize = pagingOptions.PageSize,
                First = self,
                Next = GetNextLink(self, size, pagingOptions),
                Previous = GetPreviousLink(self, size, pagingOptions),
                Last = GetLastLink(self, size, pagingOptions)
            };

        private static Link GetNextLink(Link self, int size, PagingOptions pagingOptions)
        {
            if (pagingOptions?.PageSize == null) return null;
            if (pagingOptions?.Offset == null) return null;

            var pageSize = pagingOptions.PageSize.Value;
            var offset = pagingOptions.Offset.Value;

            var next = offset + pageSize;
            if (next >= size) return null;

            var parameters = new RouteValueDictionary(self.RouteValues)
            {
                ["pagesize"] = pageSize,
                ["offset"] = next
            };

            var newLink = Link.ToCollection(self.RouteName, parameters);
            return newLink;
        }

        private static Link GetLastLink(Link self, int size, PagingOptions pagingOptions)
        {
            if (pagingOptions?.PageSize == null) return null;

            var pageSize = pagingOptions.PageSize.Value;

            if (size <= pageSize) return null;

            var offset = Math.Ceiling((size - (double)pageSize) / pageSize) * pageSize;

            var parameters = new RouteValueDictionary(self.RouteValues)
            {
                ["pagesize"] = pageSize,
                ["offset"] = offset
            };
            var newLink = Link.ToCollection(self.RouteName, parameters);

            return newLink;
        }

        private static Link GetPreviousLink(Link self, int size, PagingOptions pagingOptions)
        {
            if (pagingOptions?.PageSize == null) return null;
            if (pagingOptions?.Offset == null) return null;

            var pageSize = pagingOptions.PageSize.Value;
            var offset = pagingOptions.Offset.Value;

            if (offset == 0)
            {
                return null;
            }

            if (offset > size)
            {
                return GetLastLink(self, size, pagingOptions);
            }

            var previousPage = Math.Max(offset - pageSize, 0);

            if (previousPage <= 0)
            {
                return self;
            }

            var parameters = new RouteValueDictionary(self.RouteValues)
            {
                ["pagesize"] = pageSize,
                ["offset"] = previousPage
            };
            var newLink = Link.ToCollection(self.RouteName, parameters);

            return newLink;
        }

        #endregion
    }
}
