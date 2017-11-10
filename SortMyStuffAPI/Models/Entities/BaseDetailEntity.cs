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
        public string Label { get; set; }

        [Required]
        public DetailStyle Style { get; set; }

        [Required]
        public string CategoryId { get; set; }

        public int Position { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public virtual CategoryEntity Category { get; set; }

        public virtual ICollection<DetailEntity> Derivatives { get; set; }
    }
}
