using Epi.FormMetadata.Constants;

namespace Epi.FormMetadata.DataStructures.Interfaces
{
    public interface IAbridgedFieldInfo
    {
        string FieldName { get; }
        FieldTypes FieldType { get; }
        string List { get; }
        bool IsReadOnly { get; }
        bool IsRequired { get; }
        bool IsHidden { get; }
        bool IsDisabled { get; }
        bool IsHighlighted { get; }
    }
}
