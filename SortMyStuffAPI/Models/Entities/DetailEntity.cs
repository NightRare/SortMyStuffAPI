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
        public string BaseDetailId { get; set; }

        public DateTimeOffset ModifyTimestamp { get; set; }

        public string Field { get; set; }

        [Index(ApiStrings.IndexDetailUserId, IsClustered = true)]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual UserEntity User { get; set; }

        [ForeignKey(nameof(BaseDetailId))]
        public virtual BaseDetailEntity BaseDetail { get; set; }
    }
}
