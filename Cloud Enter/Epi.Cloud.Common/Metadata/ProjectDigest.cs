using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epi.Cloud.Common.Metadata
{
    public class ProjectDigest
    {
        public ProjectDigest()
        {
        }
        public ProjectDigest(string formName, string formId, int viewId, bool isRelatedView, int pageId, int position, string[] fieldNames = null)
        {
            FormName = formName;
            FormId = formId;
            ViewId = viewId;
            IsRelatedView = isRelatedView;
            PageId = pageId;
            Position = position;
            FieldNames = fieldNames;
        }
        public string FormName { get; set; }
        public string FormId { get; set; }
        public int ViewId { get; set; }
        public bool IsRelatedView { get; set; }
        public int PageId { get; set; }
        public int Position { get; set; }
        public string[] FieldNames { get; set; }
    }

}
