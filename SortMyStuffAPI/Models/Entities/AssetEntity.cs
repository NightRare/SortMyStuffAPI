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

        [Index("AssetContainerIndex")]
        public string ContainerId { get; set; }

        public DateTimeOffset CreateTimestamp { get; set; }

        public DateTimeOffset ModifyTimestamp { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public virtual CategoryEntity Category { get; set; }
    }
}