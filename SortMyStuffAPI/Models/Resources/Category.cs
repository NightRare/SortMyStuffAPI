using Newtonsoft.Json;
using SortMyStuffAPI.Infrastructure;

namespace SortMyStuffAPI.Models
{
    public class Category : EntityResource
    {
        [Sortable]
        [Searchable]
        public string Name { get; set; }

        public Link BaseDetails { get; set; }

        public Link CategorisedAssets { get; set; }

        public Link FormSpecs { get; set; }

    }
}
