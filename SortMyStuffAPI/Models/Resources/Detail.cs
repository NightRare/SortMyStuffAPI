using Newtonsoft.Json;
using SortMyStuffAPI.Infrastructure;

namespace SortMyStuffAPI.Models
{
    public class Detail : EntityResource
    {
        [JsonIgnore]
        [Searchable]
        public string BaseId { get; set; }

        [JsonIgnore]
        [Searchable]
        public string AssetId { get; set; }
    }
}
