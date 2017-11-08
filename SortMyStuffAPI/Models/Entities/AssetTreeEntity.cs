using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SortMyStuffAPI.Models
{
    public class AssetTreeEntity
    {
        [Key]
        public string Id { get; set; }

        public string Name { get; set; }

        public virtual List<AssetTreeEntity> Contents { get; set; }
    }
}