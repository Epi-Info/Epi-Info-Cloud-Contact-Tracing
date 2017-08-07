using System.ComponentModel.DataAnnotations;

namespace Epi.Cloud.MVC.Models
{
    public class OrganizationModel
    {
        [Required(ErrorMessage = "Organization Name is required")]
        public string Organization { get; set; }

        public string OrganizationKey { get; set; }

        public bool IsEnabled { get; set; }

        public bool IsHostOrganization { get; set; }

        public int OrganizationId { get; set; }
    }
}