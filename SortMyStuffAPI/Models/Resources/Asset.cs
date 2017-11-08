using System;
using Newtonsoft.Json;
using SortMyStuffAPI.Infrastructure;

namespace SortMyStuffAPI.Models
{
    public class Asset : Resource
    {
        [Sortable]
        [Searchable]
        public string Name { get; set; }

        [Sortable]
        [Searchable]
        public string Category { get; set; }

        [Sortable]
        public DateTimeOffset CreateTimestamp { get; set; }

        [Sortable]
        public DateTimeOffset ModifyTimestamp { get; set; }


        public Link Path { get; set; }

        public Link ContentAssets { get; set; }

        public Link Thumbnail { get; set; }

        public Link Photo { get; set; }

        public Link Details { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Link FormSpecs { get; set; }


        [JsonIgnore]
        public string Id { get; set; }

        [JsonIgnore]
        [Searchable]
        public string ContainerId { get; set; }
    }
}
