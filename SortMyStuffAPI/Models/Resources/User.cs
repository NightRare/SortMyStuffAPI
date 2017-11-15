using System;
using Newtonsoft.Json;
using SortMyStuffAPI.Infrastructure;

namespace SortMyStuffAPI.Models
{
    public class User : EntityResource
    {
        [Sortable]
        [Searchable]
        public string UserName { get; set; }

        [Sortable]
        [Searchable]
        public string Email { get; set; }

        public string Provider { get; set; }

        public DateTimeOffset CreateTimestamp { get; set; }

        public string RootAssetId { get; set; }


        [JsonIgnore]
        [Secret]
        public string Password { get; set; }

        [JsonIgnore]
        public override string UserId { get => Id; set => Id = value; }
    }
}
