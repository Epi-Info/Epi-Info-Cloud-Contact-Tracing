using Epi.Cloud.Common.Metadata.Interfaces;

namespace Epi.Cloud.Common.Metadata
{
    public class FieldDigest : IAbridgedFieldInfo
    {
        public FieldDigest()
        {
        }

        public FieldDigest(IAbridgedFieldInfo field, PageDigest pageDigest)
        {
            Field = field;
            FormName = pageDigest.FormName;
            FormId = pageDigest.FormId;
            ViewId = pageDigest.ViewId;
            IsRelatedView = pageDigest.IsRelatedView;
            PageId = pageDigest.PageId;
            Position = pageDigest.Position;
        }

        public IAbridgedFieldInfo Field { get; set; }
        public string FormName { get; set; }
        public string FormId { get; set; }
        public int ViewId { get; set; }
        public bool IsRelatedView { get; set; }
        public int PageId { get; set; }
        public int Position { get; set; }

        public string FieldName { get { return Field.FieldName; } }
        public FieldType FieldType { get { return Field.FieldType; } }
        public string List { get { return Field.List; } }
        public bool IsReadOnly { get { return Field.IsReadOnly; } }
        public bool IsRequired { get { return Field.IsRequired; } }
        public bool IsHidden { get { return Field.IsHidden; } }
        public bool IsDisabled { get { return Field.IsDisabled; } }
        public bool IsHighlighted { get { return Field.IsHighlighted; } }
    }
}
