using System.ComponentModel.DataAnnotations;

namespace SortMyStuffAPI.Models
{
    public class UpdateAssetForm : FormModel
    {
        [Required]
        [Display(Name = "name", Description = "The new name of the asset.")]
        public string Name { get; set; }
    }
}
