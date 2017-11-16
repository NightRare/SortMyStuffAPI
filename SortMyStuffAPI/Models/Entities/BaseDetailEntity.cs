using SortMyStuffAPI.Infrastructure;
using SortMyStuffAPI.Utils;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SortMyStuffAPI.Models
{
    public class BaseDetailEntity : IEntity
    {
        [Key]
        public string Id { get; set; }

        [Required]
        [Mutable]
        public string Label { get; set; }

        [Required]
        public DetailStyle Style { get; set; }

        [Required]
        public string CategoryId { get; set; }

        [Mutable]
        public int Position { get; set; }

        [Index(ApiStrings.IndexBaseDetailUserId, IsClustered = true)]
        [Required]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual UserEntity User { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public virtual CategoryEntity Category { get; set; }

        public virtual ICollection<DetailEntity> Derivatives { get; set; }
    }
}
