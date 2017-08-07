using System.Collections.Generic;
using Epi.Cloud.Common.DTO;
using Epi.Cloud.MVC.Models;

namespace Epi.Cloud.MVC.Extensions
{
    /// <summary>
    /// Maps DTO object to Model object or Model object to DTO object
    /// </summary>
    public static class OrganizationDTOExtensions
    {
        public static OrganizationModel ToOrganizationModel(this OrganizationDTO DTO)
        {
            OrganizationModel ModelList = new OrganizationModel();

            ModelList.IsEnabled = DTO.IsEnabled;
            ModelList.IsHostOrganization = DTO.IsHostOrganization;
            ModelList.Organization = DTO.Organization;
            ModelList.OrganizationId = DTO.OrganizationId;
            ModelList.OrganizationKey = DTO.OrganizationKey;
            return ModelList;
        }

        public static List<OrganizationModel> ToOrganizationModelList(this List<OrganizationDTO> list)
        {
            List<OrganizationModel> ModelList = new List<OrganizationModel>();

            foreach (var item in list)
            {
                ModelList.Add(item.ToOrganizationModel());
            }

            return ModelList;
        }
    }
}