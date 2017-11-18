using SortMyStuffAPI.Infrastructure;
using SortMyStuffAPI.Utils;
using System;
using System.ComponentModel.DataAnnotations;

namespace SortMyStuffAPI.Models
{
    public class DetailForm : RequestForm
    {
        [Required]
        [Display(Name = "field", Description = "The field of the detail.")]
        [DataSize(MinimumSize = ModelRules.DetailFieldSizeMin,
            MaximumSize = ModelRules.DetailFieldSizeMax)]
        public object Field { get; set; }

        [Required]
        [Display(Name = "modifyTimestamp", Description = "The timestamp of the last modification to this detail.")]
        public DateTimeOffset? ModifyTimestamp { get; set; }
    }
}
