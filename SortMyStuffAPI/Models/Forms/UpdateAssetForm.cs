using System.ComponentModel.DataAnnotations;

namespace SortMyStuffAPI.Models
{
    public class UpdateAssetForm : FormModel
    {
        [Required]
        [Display(Name = "name", Description = "The new name of the asset.")]
        [StringLength(maximumLength: 60, ErrorMessage = "The length of the name must be less than 60.")]
        public string Name { get; set; }
    }
}
