using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SortMyStuffAPI.Models
{
    public class AssetTreeEntity
    {
        [Key]
        public string Id { get; set; }

        public virtual IList<AssetTreeEntity> Contents { get; set; }
    }
}