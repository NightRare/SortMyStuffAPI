using SortMyStuffAPI.Utils;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SortMyStuffAPI.Models
{
    public class AssetEntity : IEntity
    {
        [Key]
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string CategoryId { get; set; }

        [Index(ApiStrings.IndexAssetContainerId)]
        public string ContainerId { get; set; }

        public DateTimeOffset CreateTimestamp { get; set; }

        public DateTimeOffset ModifyTimestamp { get; set; }

        [Index(ApiStrings.IndexAssetUserId, IsClustered = true)]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual UserEntity User { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public virtual CategoryEntity Category { get; set; }

    }
}