using System.Collections.Generic;

namespace Epi.Cloud.Common.Metadata
{
    public static class FieldMetadata
    {
        public static readonly int[] ReadonlyFieldTypes =
            { (int)FieldType.Label, (int)FieldType.Relate, (int)FieldType.Group, (int)FieldType.CommandButton, (int)FieldType.RecStatus, (int)FieldType.UniqueKey, (int)FieldType.ForeignKey, (int)FieldType.GlobalRecordId };

        static FieldMetadata()
        {
            TransformDictionary = new Dictionary<int, int>();
            TransformDictionary.Add((int)FieldType.Text, (int)FieldDataType.Text);
            TransformDictionary.Add((int)FieldType.Label, (int)FieldDataType.Text);
            TransformDictionary.Add((int)FieldType.UppercaseText, (int)FieldDataType.Text);
            TransformDictionary.Add((int)FieldType.Multiline, (int)FieldDataType.Text);
            TransformDictionary.Add((int)FieldType.Number, (int)FieldDataType.Number);
            TransformDictionary.Add((int)FieldType.Date, (int)FieldDataType.Date);
            TransformDictionary.Add((int)FieldType.Time, (int)FieldDataType.Time);
            TransformDictionary.Add((int)FieldType.DateTime, (int)FieldDataType.DateTime);
            TransformDictionary.Add((int)FieldType.Checkbox, (int)FieldDataType.Boolean);
            TransformDictionary.Add((int)FieldType.YesNo, (int)FieldDataType.YesNo);
            TransformDictionary.Add((int)FieldType.Option, (int)FieldDataType.Text);
            TransformDictionary.Add((int)FieldType.CommandButton, (int)FieldDataType.Unknown);
            TransformDictionary.Add((int)FieldType.Image, (int)FieldDataType.Object);
            TransformDictionary.Add((int)FieldType.Mirror, (int)FieldDataType.Unknown);
            TransformDictionary.Add((int)FieldType.Grid, (int)FieldDataType.Unknown);
            TransformDictionary.Add((int)FieldType.LegalValues, (int)FieldDataType.Unknown);
            TransformDictionary.Add((int)FieldType.Codes, (int)FieldDataType.Unknown);
            TransformDictionary.Add((int)FieldType.CommentLegal, (int)FieldDataType.Unknown);
            TransformDictionary.Add((int)FieldType.Relate, (int)FieldDataType.Unknown);
            TransformDictionary.Add((int)FieldType.Group, (int)FieldDataType.Unknown);
            TransformDictionary.Add((int)FieldType.RecStatus, (int)FieldDataType.Number);
            TransformDictionary.Add((int)FieldType.UniqueKey, (int)FieldDataType.Number);
            TransformDictionary.Add((int)FieldType.ForeignKey, (int)FieldDataType.Number);
            TransformDictionary.Add((int)FieldType.UniqueIdentifier, (int)FieldDataType.Guid);
            TransformDictionary.Add((int)FieldType.GlobalRecordId, (int)FieldDataType.Text);
            TransformDictionary.Add((int)FieldType.List, (int)FieldDataType.Text);
            TransformDictionary.Add((int)FieldType.Unknown, (int)FieldDataType.Text);
        }
        private static Dictionary<int, int> TransformDictionary { get; set; }
        public static FieldDataType GetDataType(FieldType fieldType)
        {
            int dataType;
            return TransformDictionary.TryGetValue((int)fieldType, out dataType) ? (FieldDataType)dataType : FieldDataType.Undefined;
        }
    }
}
