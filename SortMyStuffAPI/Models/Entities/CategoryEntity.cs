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
        public string Name { get; set; }

        public virtual ICollection<BaseDetailEntity> BaseDetails { get; set; }

        public virtual ICollection<AssetEntity> CategorisedAssets { get; set; }
    }
}
