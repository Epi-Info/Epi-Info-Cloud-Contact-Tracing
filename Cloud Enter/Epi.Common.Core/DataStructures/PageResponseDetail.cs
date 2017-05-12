using System;
using System.Collections;
using System.Collections.Generic;

namespace Epi.DataPersistence.DataStructures
{
    [Serializable]
    public partial class PageResponseDetail
    {
        public PageResponseDetail()
        {
            ResponseQA = new Dictionary<string, string>();
        }

        public string ResponseId { get; set; }
        public string FormId { get; set; }
        public string FormName { get; set; }

        public int PageId { get; set; }
        public int PageNumber { get; set; }

		public bool HasBeenUpdated { get; set; }

        public Dictionary<string, string> ResponseQA { get; set; }

		public override int GetHashCode()
		{
            int hash = 27;
            hash = (13 * hash) + (this.FormId != null ? this.FormId.GetHashCode() : 0);
            hash = (13 * hash) + (this.ResponseId != null ? this.ResponseId.GetHashCode() : 0);
            hash = (13 * hash) + (this.ResponseQA != null ? this.ResponseQA.GetHashCode() : 0);
            return hash;
		}

		public override bool Equals(object obj)
		{
			bool areEqual = false;
			var other = obj as PageResponseDetail;
			if (other != null)
			{
				areEqual = true;
				areEqual &= FormId == other.FormId;
				areEqual &= ResponseId == other.ResponseId;
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
