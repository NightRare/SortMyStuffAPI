using Newtonsoft.Json;
using SortMyStuffAPI.Infrastructure;

namespace SortMyStuffAPI.Models
{
    public class Category : Resource
    {
        [Sortable]
        [Searchable]
        public string Name { get; set; }

        public Link BaseDetails { get; set; }

        public Link CategorisedAssets { get; set; }

        [JsonIgnore]
        public string Id { get; set; }
    }
}
