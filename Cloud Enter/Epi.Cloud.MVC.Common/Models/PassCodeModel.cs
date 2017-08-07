using System.ComponentModel.DataAnnotations;

namespace Epi.Cloud.MVC.Models
{

    public class PassCodeModel
    {
        [Required]
        public string PassCode { get; set; }


    }
}