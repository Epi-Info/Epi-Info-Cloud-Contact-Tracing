
using Epi.FormMetadata.Constants;
using Epi.FormMetadata.DataStructures.Interfaces;

namespace Epi.FormMetadata.DataStructures
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

        public override int GetHashCode()
        {
            int hash = 27;
            hash = (13 * hash) + (this.FormName != null ? this.FormName.GetHashCode() : 0);
            hash = (13 * hash) + (this.FormId != null ? this.FormId.GetHashCode() : 0);
            hash = (13 * hash) + this.ViewId.GetHashCode();
            hash = (13 * hash) + this.PageId.GetHashCode();
            hash = (13 * hash) + this.Position.GetHashCode();
            return hash;
        }

        public override bool Equals(object obj)
        {
            var other = obj as FieldDigest;
            return other != null
                && other.FormName == this.FormName
                && other.FormId == this.FormId
                && other.ViewId == this.ViewId
                && other.PageId == this.PageId
                && other.Position == this.Position;
        }

        public IAbridgedFieldInfo Field { get; set; }
        public string FormName { get; set; }
        public string FormId { get; set; }
        public int ViewId { get; set; }
        public bool IsRelatedView { get; set; }
        public int PageId { get; set; }
        public int Position { get; set; }

        public string FieldName { get { return Field.FieldName; } }
		public string TrueCaseFieldName { get { return Field.TrueCaseFieldName; } }

		public FieldTypes FieldType { get { return Field.FieldType; } }
        public string List { get { return Field.List; } }
        public bool IsReadOnly { get { return Field.IsReadOnly; } }
        public bool IsRequired { get { return Field.IsRequired; } }
        public bool IsHidden { get { return Field.IsHidden; } }
        public bool IsDisabled { get { return Field.IsDisabled; } }
        public bool IsHighlighted { get { return Field.IsHighlighted; } }
    }
}
