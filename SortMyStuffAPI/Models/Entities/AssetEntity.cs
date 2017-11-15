using SortMyStuffAPI.Infrastructure;
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
        [Mutable]
        public string Name { get; set; }

        // TODO: make CategoryId changeable
        public string CategoryId { get; set; }

        [Index(ApiStrings.IndexAssetContainerId)]
        [Mutable]
        public string ContainerId { get; set; }

        public DateTimeOffset CreateTimestamp { get; set; }

        [Mutable]
        public DateTimeOffset ModifyTimestamp { get; set; }

        [Index(ApiStrings.IndexAssetUserId, IsClustered = true)]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual UserEntity User { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public virtual CategoryEntity Category { get; set; }

    }
}