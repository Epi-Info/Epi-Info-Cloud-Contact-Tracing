using System;
using Epi.Web.Enter.Common.BusinessObject;

namespace Epi.Cloud.DataEntryServices.Extensions
{
    public static class OrganizationExtensions
    {
        public static OrganizationBO ToOrganizationBO(this string Organization)
        {
            return new OrganizationBO
            {
                Organization = Organization
            };
        }
    }
}
