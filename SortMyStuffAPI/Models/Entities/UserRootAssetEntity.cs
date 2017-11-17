using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SortMyStuffAPI.Models
{
    public class UserRootAssetEntity : IEntity
    {
        [Key]
        public string Id { get; set; }

        [Required]
        [Index(IsUnique = true)]
        public string UserId { get; set; }

        [Required]
        [Index(IsUnique = true)]
        public string RootAssetId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual UserEntity User { get; set; }

        [ForeignKey(nameof(RootAssetId))]
        public virtual AssetEntity RootAsset { get; set; }
    }
}
