using System.Collections.Generic;

namespace Epi.Cloud.Common.Metadata
{
    public static class FieldMetadata
    {
        public static readonly int[] ReadonlyFieldTypes =
            { (int)FieldTypes.Label, (int)FieldTypes.Relate, (int)FieldTypes.Group, (int)FieldTypes.CommandButton, (int)FieldTypes.RecStatus, (int)FieldTypes.UniqueKey, (int)FieldTypes.ForeignKey, (int)FieldTypes.GlobalRecordId };

        static FieldMetadata()
        {
            TransformDictionary = new Dictionary<int, int>();
            TransformDictionary.Add((int)FieldTypes.Text, (int)FieldDataType.Text);
            TransformDictionary.Add((int)FieldTypes.Label, (int)FieldDataType.Text);
            TransformDictionary.Add((int)FieldTypes.UppercaseText, (int)FieldDataType.Text);
            TransformDictionary.Add((int)FieldTypes.Multiline, (int)FieldDataType.Text);
            TransformDictionary.Add((int)FieldTypes.Number, (int)FieldDataType.Number);
            TransformDictionary.Add((int)FieldTypes.Date, (int)FieldDataType.Date);
            TransformDictionary.Add((int)FieldTypes.Time, (int)FieldDataType.Time);
            TransformDictionary.Add((int)FieldTypes.DateTime, (int)FieldDataType.DateTime);
            TransformDictionary.Add((int)FieldTypes.Checkbox, (int)FieldDataType.Boolean);
            TransformDictionary.Add((int)FieldTypes.YesNo, (int)FieldDataType.YesNo);
            TransformDictionary.Add((int)FieldTypes.Option, (int)FieldDataType.Text);
            TransformDictionary.Add((int)FieldTypes.CommandButton, (int)FieldDataType.Unknown);
            TransformDictionary.Add((int)FieldTypes.Image, (int)FieldDataType.Object);
            TransformDictionary.Add((int)FieldTypes.Mirror, (int)FieldDataType.Unknown);
            TransformDictionary.Add((int)FieldTypes.Grid, (int)FieldDataType.Unknown);
            TransformDictionary.Add((int)FieldTypes.LegalValues, (int)FieldDataType.Unknown);
            TransformDictionary.Add((int)FieldTypes.Codes, (int)FieldDataType.Unknown);
            TransformDictionary.Add((int)FieldTypes.CommentLegal, (int)FieldDataType.Unknown);
            TransformDictionary.Add((int)FieldTypes.Relate, (int)FieldDataType.Unknown);
            TransformDictionary.Add((int)FieldTypes.Group, (int)FieldDataType.Unknown);
            TransformDictionary.Add((int)FieldTypes.RecStatus, (int)FieldDataType.Number);
            TransformDictionary.Add((int)FieldTypes.UniqueKey, (int)FieldDataType.Number);
            TransformDictionary.Add((int)FieldTypes.ForeignKey, (int)FieldDataType.Number);
            TransformDictionary.Add((int)FieldTypes.UniqueIdentifier, (int)FieldDataType.Guid);
            TransformDictionary.Add((int)FieldTypes.GlobalRecordId, (int)FieldDataType.Text);
            TransformDictionary.Add((int)FieldTypes.List, (int)FieldDataType.Text);
            TransformDictionary.Add((int)FieldTypes.Unknown, (int)FieldDataType.Text);
        }
        private static Dictionary<int, int> TransformDictionary { get; set; }
        public static FieldDataType GetDataType(FieldTypes fieldType)
        {
            int dataType;
            return TransformDictionary.TryGetValue((int)fieldType, out dataType) ? (FieldDataType)dataType : FieldDataType.Undefined;
        }
    }
}
