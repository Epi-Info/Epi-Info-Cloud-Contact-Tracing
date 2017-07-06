namespace Epi.Cloud.Common.BusinessObjects
{
    public class OrganizationBO
    {
		public int OrganizationId { get; set; }

        public string Organization { get; set; }

		public string OrganizationKey { get; set; }

		public bool IsEnabled { get; set; }

        public bool IsHostOrganization { get; set; }
    }
}
