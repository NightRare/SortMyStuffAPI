﻿using SortMyStuffAPI.Infrastructure;

namespace SortMyStuffAPI.Models
{
    public class Asset : Resource
    {
        [Sortable]
        public string Name { get; set; }

        public Link Container { get; set; }

        [Sortable]
        public string Category { get; set; }

        [Sortable]
        public string CreateTimestamp { get; set; }

        [Sortable]
        public string ModifyTimestamp { get; set; }

        public Link ThumbnailUrl { get; set; }

        public Link Details { get; set; }

    }
}
