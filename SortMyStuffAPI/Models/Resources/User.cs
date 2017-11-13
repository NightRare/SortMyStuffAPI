using System;
using Newtonsoft.Json;
using SortMyStuffAPI.Infrastructure;

namespace SortMyStuffAPI.Models
{
    public class User : Resource
    {
        [Sortable]
        [Searchable]
        public string UserName { get; set; }

        [Sortable]
        [Searchable]
        public string Email { get; set; }

        public string Provider { get; set; }

        public DateTimeOffset CreateTimestamp { get; set; }

        [JsonIgnore]
        public string Id { get; set; }

        [JsonIgnore]
        [Secret]
        public string Password { get; set; }
    }
}
