namespace Epi.Cloud.Common.Metadata
{
    public class FieldDigest
    {
        public FieldDigest()
        {
        }
        public FieldDigest(string fieldName, ProjectDigest projectDigest)
        {
            Name = fieldName;
            FormName = projectDigest.FormName;
            FormId = projectDigest.FormId;
            ViewId = projectDigest.ViewId;
            IsRelatedView = projectDigest.IsRelatedView;
            PageId = projectDigest.PageId;
            Position = projectDigest.Position;
        }

        public string Name { get; set; }
        public string FormName { get; set; }
        public string FormId { get; set; }
        public int ViewId { get; set; }
        public bool IsRelatedView { get; set; }
        public int PageId { get; set; }
        public int Position { get; set; }
    }
}
