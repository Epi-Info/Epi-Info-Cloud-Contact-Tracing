using System;
using Epi.Cloud.Common.BusinessObjects;

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
