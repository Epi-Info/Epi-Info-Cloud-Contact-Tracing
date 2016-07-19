using Epi.Cloud.Common.Utilities;
using Newtonsoft.Json;

namespace Epi.Cloud.Common.Metadata
{
    public partial class FormDigest
    {
        public int ViewId { get; set; }
        public string FormId { get; set; }
        public string FormName { get; set; }
        public string ParentFormId { get; set; }
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public string OrganizationKey { get; set; }
        public int OwnerUserId { get; set; }
        public int NumberOfPages { get; set; }
        public string Orientation { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }

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

        public int DataAccessRuleId { get; set; }
        public bool IsDraftMode { get; set; }
    }
}
