namespace Epi.Cloud.Common.DTO
{
    public class OrganizationDTO
    {
        public string Organization { get; set; }

        public string OrganizationKey { get; set; }

		public bool IsEnabled { get; set; }

        public bool IsHostOrganization { get; set; }

        public int OrganizationId { get; set; }
	}
}
