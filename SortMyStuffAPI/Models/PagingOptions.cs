using System.ComponentModel.DataAnnotations;

namespace SortMyStuffAPI.Models
{
    public class PagingOptions
    {
        [Range(1, 99999, ErrorMessage = "Offset parameter must be between 1 to 99999.")]
        public int? Offset { get; set; }

        [Range(1, 10000, ErrorMessage = "PageSize parameter must be between 1 to 10000.")]
        public int? PageSize { get; set; }
    }
}
