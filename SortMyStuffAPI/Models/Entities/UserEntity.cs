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

        public string RootAssetContractId { get; set; }


        [ForeignKey(nameof(RootAssetContractId))]
        public UserRootAssetEntity RootAssetContract { get; set; }

        public virtual ICollection<AssetEntity> Assets { get; set; }

        public virtual ICollection<CategoryEntity> Categories { get; set; }

        public virtual ICollection<BaseDetailEntity> BaseDetails { get; set; }

        public virtual ICollection<DetailEntity> Details { get; set; }

        [NotMapped]
        public string UserId
        {
            get { return Id; }
            set { }
        }

        [NotMapped]
        public string RootAssetId
        {
            get { return RootAssetContract?.RootAssetId; }
            set { }
        }

        [NotMapped]
        public AssetEntity RootAsset
        {
            get { return RootAssetContract?.RootAsset; }
            set { }
        }
    }
}
