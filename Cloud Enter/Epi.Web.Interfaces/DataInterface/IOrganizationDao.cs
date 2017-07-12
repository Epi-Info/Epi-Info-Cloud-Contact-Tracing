using System.Collections.Generic;
using Epi.Web.Enter.Common.BusinessObject;

namespace Epi.Web.Enter.Interfaces.DataInterfaces
{
    /// <summary>
    /// Defines methods to access Organizations.
    /// </summary>
    /// <remarks>
    /// This is a database-independent interface. Implementations are database specific.
    /// </remarks>
    public interface IOrganizationDao
    {
        List<OrganizationBO> GetOrganizationKeys(string OrganizationName);
        List<OrganizationBO> GetOrganizationInfoByOrgKey(string gOrgKeyEncrypted);
        List<OrganizationBO> GetOrganizationInfo();
        List<OrganizationBO> GetOrganizationNames();
        OrganizationBO GetOrganizationInfoByKey(string key);

        /// <summary>
        /// Inserts a new Organization. 
        /// </summary>
        /// <param name="Organization">Organization.</param>
        /// <remarks>
        /// Following insert, Organization object will contain the new identifier.
        /// </remarks>
        void InsertOrganization(OrganizationBO Organization);

        /// <summary>
        /// Updates a Organization.
        /// </summary>
        /// <param name="Organization">Organization.</param>
        bool UpdateOrganization(OrganizationBO Organization);

        /// <summary>
        /// Deletes a Organization
        /// </summary>
        /// <param name="Organization">Organization.</param>
         void DeleteOrganization(OrganizationBO Organization);


         List<OrganizationBO> GetOrganizationInfoByUserId(int UserId, int UserRole);

        /// <summary>
        /// Brings all the organizations user is part of.
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
         List<OrganizationBO> GetOrganizationsByUserId(int UserId);

         List<OrganizationBO> GetOrganizationInfoForAdmin(int UserId, int UserRole);
         bool InsertOrganization(OrganizationBO Organization,UserBO User);
         UserBO GetUserByEmail(UserBO User);
         bool InsertOrganization(OrganizationBO Organization, int UserId,int RoleId);
         

         OrganizationBO GetOrganizationByOrgId(int OrganizationId);
         bool IsUserExistsInOrganization(string OrgKey, int UserId);
    }
}
