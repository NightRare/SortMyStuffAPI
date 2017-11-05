using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SortMyStuffAPI.Models.Entities
{
    public class AssetTreeEntity
    {
        [Key]
        public string Id { get; set; }

        public virtual IList<AssetTreeEntity> Contents { get; set; }
    }
}