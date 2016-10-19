using System.Runtime.Serialization;

namespace Epi.Cloud.Common.BusinessObjects
{
    public class FormInfoBO
    {
        public int DataAccesRuleId { get; set; }

        public string FormId { get; set; }

		public string FormNumber { get; set; }

		public string FormName { get; set; }

		public string OrganizationName { get; set; }

		public int OrganizationId { get; set; }

		public bool IsDraftMode { get; set; }

		public int UserId { get; set; }

		public bool IsOwner { get; set; }

		public string OwnerLName { get; set; }

		public string OwnerFName { get; set; }

		public string ParentId { get; set; }

		public bool EwavLiteToggleSwitch { get; set; }

		public bool IsSQLProject { get; set; }

		public bool IsShareable { get; set; }

		public bool IsShared { get; set; }

		public bool HasDraftModeData { get; set; }
	}
}
