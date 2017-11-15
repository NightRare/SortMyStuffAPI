using SortMyStuffAPI.Infrastructure;
using SortMyStuffAPI.Utils;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SortMyStuffAPI.Models
{
    public class CategoryEntity : IEntity
    {
        [Key]
        public string Id { get; set; }

        [Required]
        [Index]
        [Mutable]
        public string Name { get; set; }

        [Index(ApiStrings.IndexCategoryUserId, IsClustered = true)]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual UserEntity User { get; set; }

        public virtual ICollection<BaseDetailEntity> BaseDetails { get; set; }

        public virtual ICollection<AssetEntity> CategorisedAssets { get; set; }
    }
}
