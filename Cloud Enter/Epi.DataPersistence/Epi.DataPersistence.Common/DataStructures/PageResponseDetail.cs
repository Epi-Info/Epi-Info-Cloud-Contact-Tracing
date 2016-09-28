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

        public Dictionary<string, string> ResponseQA { get; set; }
    }
}
