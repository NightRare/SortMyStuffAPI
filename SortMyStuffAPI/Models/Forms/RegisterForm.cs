using System.ComponentModel.DataAnnotations;
using SortMyStuffAPI.Infrastructure;
using SortMyStuffAPI.Utils;

namespace SortMyStuffAPI.Models
{
    public class RegisterForm : FormModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "email", Description = "Email address")]
        public string Email { get; set; }

        [Required]
        [MinLength(ModelRules.UserPasswordLengthMin)]
        [MaxLength(ModelRules.UserPasswordLengthMax)]
        [Secret]
        [Display(Name = "password", Description = "Password")]
        public string Password { get; set; }

        [Required]
        [MinLength(ModelRules.UserNameLengthMin)]
        [MaxLength(ModelRules.UserNameLengthMax)]
        [Display(Name = "userName", Description = "UserName")]
        public string UserName { get; set; }
    }
}
