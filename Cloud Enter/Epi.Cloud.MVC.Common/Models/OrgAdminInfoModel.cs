using System.ComponentModel.DataAnnotations;

namespace Epi.Cloud.MVC.Models
{
    public class OrgAdminInfoModel
    {
        public OrgAdminInfoModel()
        {
        }

        [Required(ErrorMessage = "Email is required.")]
        [RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$", ErrorMessage = "Invalid email address.")]
        public string AdminEmail { get; set; }

        [Required(ErrorMessage = "The organization name is required.")]
        public string OrgName { get; set; }

        [Required(ErrorMessage = "Confirm email is required.")]
        [RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$", ErrorMessage = "Invalid email address.")]
        [System.ComponentModel.DataAnnotations.Compare("AdminEmail", ErrorMessage = "The email and confirmation do not match.")]
        public string ConfirmAdminEmail { get; set; }

        public string Status { get; set; }

        [Required(ErrorMessage = "First Name is required.")]
        public string AdminFirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required.")]
        public string AdminLastName { get; set; }

        public bool IsEditMode { get; set; }

        public bool IsOrgEnabled { get; set; }

        public bool IsHostOrganization { get; set; }
    }
}