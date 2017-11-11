using System.ComponentModel.DataAnnotations;
using SortMyStuffAPI.Utils;

namespace SortMyStuffAPI.Models
{
    public class CategoryForm : FormModel
    {
        [Required]
        [Display(Name = "name", Description = "The new name of the asset.")]
        [StringLength(maximumLength: ModelRules.CategoryNameLength, ErrorMessage = ModelRules.CategoryNameLengthErrorMessage)]
        public string Name { get; set; }
    }
}
