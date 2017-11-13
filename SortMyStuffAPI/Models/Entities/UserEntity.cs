using System;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SortMyStuffAPI.Models
{
    public class UserEntity : IdentityUser<string>, IEntity
    {
        public AuthProvider Provider { get; set; }
            = AuthProvider.Native;

        public DateTimeOffset CreateTimestamp { get; set; }

        public string RootAssetId { get; set; }


        [ForeignKey(nameof(RootAssetId))]
        public virtual AssetEntity RootAsset { get; set; }

        public virtual ICollection<AssetEntity> Assets { get; set; }

        public virtual ICollection<CategoryEntity> Categories { get; set; }

        public virtual ICollection<BaseDetailEntity> BaseDetails { get; set; }

        public virtual ICollection<DetailEntity> Details { get; set; }

    }
}
