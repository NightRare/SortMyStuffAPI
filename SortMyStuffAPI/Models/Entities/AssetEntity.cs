using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SortMyStuffAPI.Models.Entities
{
    public class AssetEntity
    {
        [Key]
        public string Id { get; set; }

        public string Name { get; set; }

        public string ContainerId { get; set; }

        public string Category { get; set; }

        public long CreateTimestamp { get; set; }

        public long ModifyTimestamp { get; set; }

        public string ThumbnailUrl { get; set; }
        
//        public virtual IList<string> DetailsList { get; set; }
    }
}