using Newtonsoft.Json;
using SortMyStuffAPI.Infrastructure;
using System;

namespace SortMyStuffAPI.Models
{
    public class Detail : EntityResource
    {
        public object Field { get; set; }

        [Sortable]
        public DateTimeOffset ModifyTimestamp { get; set; }

        public Link Asset { get; set; }

        public Link BaseDetail { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Link FormSpecs { get; set; }

        [JsonIgnore]
        [Searchable]
        public string BaseId { get; set; }

        [JsonIgnore]
        [Searchable]
        public string AssetId { get; set; }
    }
}
