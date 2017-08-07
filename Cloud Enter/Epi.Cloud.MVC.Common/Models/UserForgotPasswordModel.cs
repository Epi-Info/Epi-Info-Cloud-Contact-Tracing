using System.ComponentModel.DataAnnotations;

namespace Epi.Cloud.MVC.Models
{
    public class UserForgotPasswordModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$", ErrorMessage = "Invalid email address.")]
        public string UserName { get; set; }
    }
}