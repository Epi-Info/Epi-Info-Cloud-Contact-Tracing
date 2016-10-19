using System.Runtime.Serialization;

namespace Epi.Web.Enter.Common.BusinessObject
{
    public class FormInfoBO
    {
        [DataMember]
		public int DataAccesRuleId { get; set; }

        [DataMember]
        public string FormId { get; set; }

		[DataMember]
        public string FormNumber { get; set; }

		[DataMember]
        public string FormName { get; set; }

		[DataMember]
        public string OrganizationName { get; set; }

		[DataMember]
        public int OrganizationId { get; set; }

		[DataMember]
        public bool IsDraftMode { get; set; }

		[DataMember]
        public int UserId { get; set; }

		[DataMember]
        public bool IsOwner { get; set; }

		[DataMember]
        public string OwnerLName { get; set; }

		[DataMember]
        public string OwnerFName { get; set; }

		[DataMember]
        public string Xml { get; set; }

		[DataMember]
        public string ParentId { get; set; }

		[DataMember]
        public bool EwavLiteToggleSwitch { get; set; }

		public bool IsSQLProject { get; set; }

		public bool IsShareable { get; set; }

		public bool IsShared { get; set; }

		public bool HasDraftModeData { get; set; }

	}
}
