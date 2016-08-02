using System.Linq;

namespace Epi.Cloud.Common.Metadata
{
    public class ProjectDigest
    {
        public ProjectDigest()
        {
            Fields = new AbridgedFieldInfo[0];
        }

        public ProjectDigest(string formName, string formId, int viewId, bool isRelatedView, int pageId, int position, Field[] fields) :
            this(formName, formId, viewId, isRelatedView, pageId, position,
                 fields != null ? fields.Select(f => new AbridgedFieldInfo(f)).ToArray() : new AbridgedFieldInfo[0])
        {
        }

        public ProjectDigest(string formName, string formId, int viewId, bool isRelatedView, int pageId, int position, AbridgedFieldInfo[] fields)
        {
            FormName = formName;
            FormId = formId;
            ViewId = viewId;
            IsRelatedView = isRelatedView;
            PageId = pageId;
            Position = position;
            Fields = fields != null ? fields.ToArray() : new AbridgedFieldInfo[0];
        }

        public string FormName { get; set; }
        public string FormId { get; set; }
        public int ViewId { get; set; }
        public bool IsRelatedView { get; set; }
        public int PageId { get; set; }
        public int Position { get; set; }
        public AbridgedFieldInfo[] Fields { get; set; }
        public bool IsReadonly { get { return Fields.Any(f => !FieldType.ReadonlyFieldTypes.Contains(f.FieldType)); } }
        public string[] FieldNames
        {
            get { return Fields.Select(f => f.Name).ToArray(); }
        }
    }
}
