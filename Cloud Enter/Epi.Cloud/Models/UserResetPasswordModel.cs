using System.ComponentModel.DataAnnotations;

namespace Epi.Web.MVC.Models
{
    public class UserResetPasswordModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }

        [System.ComponentModel.DataAnnotations.Compare("Password")]
        public string ConfirmPassword { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int MinimumLength { get; set; }

        public int MaximumLength { get; set; }

        public string Symbols { get; set; }

        public bool UseSymbols { get; set; }

        public bool UseNumeric { get; set; }

        public bool UseLowerCase { get; set; }

        public bool UseUpperCase { get; set; }

        public bool UseUserIdInPassword { get; set; }

        public bool UseUserNameInPassword { get; set; }

        public int NumberOfTypesRequiredInPassword { get; set; }
    }
}