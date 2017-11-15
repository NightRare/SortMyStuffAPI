using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using SortMyStuffAPI.Utils;
using SortMyStuffAPI.Infrastructure;

namespace SortMyStuffAPI.Models
{
    public class BaseAssetForm : RequestForm
    {
        [Required]
        [Display(Name = "name", Description = "The new name of the asset.")]
        [StringLength(maximumLength: ModelRules.AssetNameLength, ErrorMessage = ModelRules.AssetNameLengthErrorMessage)]
        public string Name { get; set; }

        // TODO: make category changeable
        [Immutable]
        [Required]
        [Display(Name = "categoryId", Description = "The id of the category of the asset.")]
        public string CategoryId { get; set; }

        [Required]
        [Display(Name = "containerId", Description = "The id of the container asset.")]
        public string ContainerId { get; set; }
    }
}
