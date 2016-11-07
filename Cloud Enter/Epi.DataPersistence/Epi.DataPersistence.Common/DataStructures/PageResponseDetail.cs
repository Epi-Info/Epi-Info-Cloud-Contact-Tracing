using System.Collections.Generic;

namespace Epi.DataPersistence.DataStructures
{
    public partial class PageResponseDetail
    {
        public PageResponseDetail()
        {
            ResponseQA = new Dictionary<string, string>();
        }

        public string FormId { get; set; }
        public string FormName { get; set; }

        public int PageId { get; set; }
        public int PageNumber { get; set; }

        public string GlobalRecordID { get; set; }

		public bool HasBeenUpdated { get; set; }

        public Dictionary<string, string> ResponseQA { get; set; }

		public override int GetHashCode()
		{
			return FormId != null ? FormId.GetHashCode() : 0
				+ GlobalRecordID != null ? GlobalRecordID.GetHashCode() : 0 
				+ ResponseQA.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			bool areEqual = false;
			var other = obj as PageResponseDetail;
			if (other != null)
			{
				areEqual = true;
				areEqual &= FormId == other.FormId;
				areEqual &= GlobalRecordID == other.GlobalRecordID;
				areEqual &= ResponseQA.Count == other.ResponseQA.Count;
				if (areEqual)
				{
					string value;
					foreach (var key in ResponseQA.Keys)
					{
						areEqual &= other.ResponseQA.TryGetValue(key, out value);
						if (!areEqual) break;
						areEqual &= ResponseQA[key] == value;
						if (!areEqual) break;
					}
				}
			}
			return areEqual;
		}
	}
}
