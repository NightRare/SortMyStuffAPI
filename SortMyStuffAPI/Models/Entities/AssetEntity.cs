using System.ComponentModel.DataAnnotations;

namespace SortMyStuffAPI.Models
{
    public class AssetEntity
    {
        [Key]
        public string Id { get; set; }

        public string Name { get; set; }

        public string ContainerId { get; set; }

        public string Category { get; set; }

        public string CreateTimestamp { get; set; }

        public string ModifyTimestamp { get; set; }

    }
}