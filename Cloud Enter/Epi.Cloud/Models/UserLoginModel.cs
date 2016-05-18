using System.ComponentModel.DataAnnotations;

namespace Epi.Web.MVC.Models
{
    public class UserLoginModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }

    }
}