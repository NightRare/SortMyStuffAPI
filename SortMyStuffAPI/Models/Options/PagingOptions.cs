using System.ComponentModel.DataAnnotations;
using SortMyStuffAPI.Utils;

namespace SortMyStuffAPI.Models
{
    public class PagingOptions
    {
        [Range(ModelRules.PagingOffsetMin, 
            ModelRules.PagingOffsetMax, 
            ErrorMessage = ModelRules.PagingOffsetErrorMessage)]
        public int? Offset { get; set; }

        [Range(ModelRules.PagingPageSizeMin, 
            ModelRules.PagingPageSizeMax, 
            ErrorMessage = ModelRules.PagingPageSizeErrorMessage)]
        public int? PageSize { get; set; }
    }
}
