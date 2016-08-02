namespace Epi.Cloud.Common.Metadata
{
    public class FieldDigest
    {
        public FieldDigest()
        {
        }

        public FieldDigest(AbridgedFieldInfo field, ProjectDigest projectDigest)
        {
            Field = field;
            FormName = projectDigest.FormName;
            FormId = projectDigest.FormId;
            ViewId = projectDigest.ViewId;
            IsRelatedView = projectDigest.IsRelatedView;
            PageId = projectDigest.PageId;
            Position = projectDigest.Position;
        }

        public AbridgedFieldInfo Field { get; set; }
        public string FormName { get; set; }
        public string FormId { get; set; }
        public int ViewId { get; set; }
        public bool IsRelatedView { get; set; }
        public int PageId { get; set; }
        public int Position { get; set; }

        public string Name { get { return Field.Name; } }
        public int FieldType { get { return Field.FieldType; } }
        public int DataType { get { return FieldTypeToDataType.GetDataType(FieldType); } }
        public bool IsReadonly { get { return Field.IsReadonly; } }
    }
}
