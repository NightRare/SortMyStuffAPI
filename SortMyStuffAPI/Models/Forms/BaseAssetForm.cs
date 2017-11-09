using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SortMyStuffAPI.Models
{
    public class BaseAssetForm : FormModel
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
    }
}
