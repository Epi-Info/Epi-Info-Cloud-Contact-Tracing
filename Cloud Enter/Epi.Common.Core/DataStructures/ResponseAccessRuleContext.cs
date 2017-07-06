using System;
namespace Epi.Cloud.Common.Core.DataStructures
{
    public class ResponseAccessRuleContext
    {
        public bool IsSharable { get; set; }
        public int RuleId { get; set; }
        public bool IsHostOrganizationUser { get; set; }
        public int UserOrganizationId { get; set; }
    }
}
