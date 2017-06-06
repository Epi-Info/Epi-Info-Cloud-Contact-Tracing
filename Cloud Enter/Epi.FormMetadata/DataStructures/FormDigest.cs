using System.Collections.Generic;
using Epi.FormMetadata.Utilities;
using Newtonsoft.Json;

namespace Epi.FormMetadata.DataStructures
{
    public partial class FormDigest
    {
        public string FormId { get; set; }
        public string FormName { get; set; }
        public string ParentFormId { get; set; }
        public string RootFormId { get; set; }
        public int ViewId { get; set; }
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public string OrganizationKey { get; set; }
        public int OwnerUserId { get; set; }
        public int NumberOfPages { get; set; }
        public string Orientation { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }

        public Dictionary<string, int> FieldNameToPageIdDirectory { get; set; }

        public int FieldNameToPageId(string fieldName)
        {
            int pageId = 0;
            pageId = FieldNameToPageIdDirectory.TryGetValue(fieldName.ToLower(), out pageId) ? pageId : 0;
            return pageId;
        }

        [JsonProperty]
        private string _compressedCheckCode = null;

        [JsonIgnore]
        public string CheckCode
        {
            get
            {
                return !string.IsNullOrWhiteSpace(_compressedCheckCode) ? StringCompressor.DecompressString(_compressedCheckCode) : null;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _compressedCheckCode = StringCompressor.CompressString(value.Trim());
                }
                else
                {
                    _compressedCheckCode = null;
                }
            }
        }

        public bool IsShareable { get; set; }
        public int DataAccessRuleId { get; set; }
        public bool IsDraftMode { get; set; }

        [JsonIgnore]
        public bool IsRootForm { get { return FormId == RootFormId; } }
        public bool IsChildForm { get { return FormId != RootFormId; } }
    }
}
