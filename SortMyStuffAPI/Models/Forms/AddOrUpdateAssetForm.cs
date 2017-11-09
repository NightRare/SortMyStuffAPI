using System;
using System.ComponentModel.DataAnnotations;

namespace SortMyStuffAPI.Models
{
    public class AddOrUpdateAssetForm : BaseAssetForm
    {
        [Required]
        [Display(Name = "createTimestamp", Description = "The timestamp of the creation of this asset.")]
        public DateTimeOffset? CreateTimestamp { get; set; }

        [Required]
        [Display(Name = "modifyTimestamp", Description = "The timestamp of the last modification to this asset.")]
        public DateTimeOffset? ModifyTimestamp { get; set; }
    }
}
