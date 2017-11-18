using SortMyStuffAPI.Infrastructure;
using SortMyStuffAPI.Utils;
using System.ComponentModel.DataAnnotations;

namespace SortMyStuffAPI.Models
{
    public class BaseDetailForm : RequestForm
    {
        [Required]
        [Display(Name = "label", Description = "The label of the base detail.")]
        [StringLength(maximumLength: ModelRules.DetailLabelLength, 
            ErrorMessage = ModelRules.DetailLabelLengthErrorMessage)]
        [ScopedUnique(Scope = Scope.Category)]
        public string Label { get; set; }

        [Required]
        [Display(Name = "style", Description = "The style of the detail.")]
        [Option(typeof(DetailStyle))]
        [Immutable]
        public string Style { get; set; }

        [Required]
        [Display(Name = "position", Description = "The relative position represented by an integer.")]
        [ScopedUnique(Scope = Scope.Category)]
        public int Position { get; set; }

        [Required]
        [Display(Name = "categoryId", Description = "The Id of the associated category.")]
        [Immutable]
        public string CategoryId { get; set; }
    }
}
