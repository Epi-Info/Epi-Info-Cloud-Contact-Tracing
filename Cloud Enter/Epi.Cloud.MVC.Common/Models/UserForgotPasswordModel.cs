using System.ComponentModel.DataAnnotations;

namespace Epi.Cloud.MVC.Models
{
    public class UserForgotPasswordModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$", ErrorMessage = "The email address you entered is not in the proper format.")]
        public string UserName { get; set; }
    }
}