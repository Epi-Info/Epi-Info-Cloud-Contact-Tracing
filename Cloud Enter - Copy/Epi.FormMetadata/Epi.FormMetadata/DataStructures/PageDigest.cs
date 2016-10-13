using System.Linq;

namespace Epi.FormMetadata.DataStructures
{
    public class PageDigest
    {
        public PageDigest()
        {
            Fields = new AbridgedFieldInfo[0];
        }

        public PageDigest(string pageName, int pageId, int position, string formId, string formName, int viewId, bool isRelatedView, Field[] fields) :
            this(formName, formId, viewId, isRelatedView, pageName, pageId, position,
                 fields != null ? fields.Select(f => new AbridgedFieldInfo(f)).ToArray() : new AbridgedFieldInfo[0])
        {
        }

        public PageDigest(string formName, string formId, int viewId, bool isRelatedView, string pageName, int pageId, int position, AbridgedFieldInfo[] fields)
        {
            FormName = formName;
            FormId = formId;
            ViewId = viewId;
            IsRelatedView = isRelatedView;
            PageName = pageName;
            PageId = pageId;
            Position = position;
            Fields = fields != null ? fields.ToArray() : new AbridgedFieldInfo[0];
        }

        public string FormName { get; set; }
        public string FormId { get; set; }
        public int ViewId { get; set; }
        public bool IsRelatedView { get; set; }
        public string PageName { get; set; }
        public int PageId { get; set; }
        public int Position { get; set; }
        public int DataAccessRuleId { get; set; }
        public AbridgedFieldInfo[] Fields { get; set; }
        public bool IsReadonly { get { return !Fields.Any(f => !f.IsReadOnly); } }
        public string[] FieldNames
        {
            get { return Fields.Select(f => f.FieldName).ToArray(); }
        }

        public int PageNumber { get { return Position + 1; } }
    }
}
