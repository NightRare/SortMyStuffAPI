using Newtonsoft.Json;
using SortMyStuffAPI.Infrastructure;

namespace SortMyStuffAPI.Models
{
    public class Asset : Resource
    {

        [Sortable]
        [Searchable]
        public string Name { get; set; }

        public Link Container { get; set; }

        [Sortable]
        [Searchable]
        public string Category { get; set; }

        [Sortable]
        public string CreateTimestamp { get; set; }

        [Sortable]
        public string ModifyTimestamp { get; set; }
         
        public Link Thumbnail { get; set; }

        public Link Photo { get; set; }

        public Link Details { get; set; }

        public Form UpdateAsset { get; set; }

    }
}
