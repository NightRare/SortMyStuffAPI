using System.ComponentModel.DataAnnotations;
using SortMyStuffAPI.Utils;
using SortMyStuffAPI.Infrastructure;

namespace SortMyStuffAPI.Models
{
    public class CategoryForm : RequestForm
    {
        [Required]
        [ScopedUnique(Scope = Scope.User)]
        [Display(Name = "name", Description = "The new name of the asset.")]
        [StringLength(maximumLength: ModelRules.CategoryNameLength, ErrorMessage = ModelRules.CategoryNameLengthErrorMessage)]
        public string Name { get; set; }
    }
}
