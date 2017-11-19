using SortMyStuffAPI.Infrastructure;
using SortMyStuffAPI.Utils;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SortMyStuffAPI.Models
{
    public class DetailEntity : IEntity
    {
        [Key]
        public string Id { get; set; }

        [Required]
        public string AssetId { get; set; }

        [Required]
        public string BaseId { get; set; }

        [Mutable]
        public string Field { get; set; }

        [Required]
        [Mutable]
        public DateTimeOffset ModifyTimestamp { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey(nameof(AssetId))]
        public virtual AssetEntity Asset { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual UserEntity User { get; set; }

        [ForeignKey(nameof(BaseId))]
        public virtual BaseDetailEntity BaseDetail { get; set; }
    }
}
