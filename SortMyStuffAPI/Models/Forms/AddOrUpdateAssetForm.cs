using System;
using System.ComponentModel.DataAnnotations;

namespace SortMyStuffAPI.Models
{
    public class AddOrUpdateAssetForm : FormModel
    {
        [Required]
        [Display(Name = "name", Description = "The new name of the asset.")]
        [StringLength(maximumLength: 60, ErrorMessage = "The length of the name must be less than 60.")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "category", Description = "The category of the asset.")]
        public string Category { get; set; }

        [Required]
        [Display(Name = "containerId", Description = "The id of the container asset.")]
        public string ContainerId { get; set; }

        [Required]
        [Display(Name = "createTimestamp", Description = "The timestamp of the creation of this asset.")]
        public DateTimeOffset? CreateTimestamp { get; set; }

        [Required]
        [Display(Name = "modifyTimestamp", Description = "The timestamp of the last modification to this asset.")]
        public DateTimeOffset? ModifyTimestamp { get; set; }
    }
}
