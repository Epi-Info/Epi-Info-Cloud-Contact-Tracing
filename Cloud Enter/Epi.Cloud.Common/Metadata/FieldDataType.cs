using System;
using System.Collections.Generic;

namespace Epi.Cloud.Common.Metadata
{
    public struct FieldDataType
    {
        public const int Undefined = -1,
                         Object = 0,
                         Number = 1,
                         Text = 2,
                         Date = 3,
                         Time = 4,
                         DateTime = 5,
                         Boolean = 6,
                         PhoneNumber = 7,
                         YesNo = 8,
                         Unknown = 9,
                         Guid = 10,
                         Class = 11,
                         Function = 12;

    }

    public static class FieldTypeToDataType
    {
        static FieldTypeToDataType()
        {
            TransformDictionary = new Dictionary<int, int>();
            TransformDictionary.Add(FieldType.Text, FieldDataType.Text);
            TransformDictionary.Add(FieldType.Label, FieldDataType.Text);
            TransformDictionary.Add(FieldType.UppercaseText, FieldDataType.Text);
            TransformDictionary.Add(FieldType.Multiline, FieldDataType.Text);
            TransformDictionary.Add(FieldType.Number, FieldDataType.Number);
            TransformDictionary.Add(FieldType.Date, FieldDataType.Date);
            TransformDictionary.Add(FieldType.Time, FieldDataType.Time);
            TransformDictionary.Add(FieldType.DateTime, FieldDataType.DateTime);
            TransformDictionary.Add(FieldType.Checkbox, FieldDataType.Boolean);
            TransformDictionary.Add(FieldType.YesNo, FieldDataType.YesNo);
            TransformDictionary.Add(FieldType.Option, FieldDataType.Text);
            TransformDictionary.Add(FieldType.CommandButton, FieldDataType.Unknown);
            TransformDictionary.Add(FieldType.Image, FieldDataType.Object);
            TransformDictionary.Add(FieldType.Mirror, FieldDataType.Unknown);
            TransformDictionary.Add(FieldType.Grid, FieldDataType.Unknown);
            TransformDictionary.Add(FieldType.LegalValues, FieldDataType.Unknown);
            TransformDictionary.Add(FieldType.Codes, FieldDataType.Unknown);
            TransformDictionary.Add(FieldType.CommentLegal, FieldDataType.Unknown);
            TransformDictionary.Add(FieldType.Relate, FieldDataType.Unknown);
            TransformDictionary.Add(FieldType.Group, FieldDataType.Unknown);
            TransformDictionary.Add(FieldType.RecStatus, FieldDataType.Number);
            TransformDictionary.Add(FieldType.UniqueKey, FieldDataType.Number);
            TransformDictionary.Add(FieldType.ForeignKey, FieldDataType.Number);
            TransformDictionary.Add(FieldType.UniqueIdentifier, FieldDataType.Guid);
            TransformDictionary.Add(FieldType.GlobalRecordId, FieldDataType.Text);
            TransformDictionary.Add(FieldType.List, FieldDataType.Text);
            TransformDictionary.Add(FieldType.Unknown, FieldDataType.Text);
        }
        public static Dictionary<int, int> TransformDictionary { get; private set; }
        public static int GetDataType(int fieldType)
        {
            int dataType;
            return TransformDictionary.TryGetValue(fieldType, out dataType) ? dataType : FieldDataType.Undefined;
        }
    }

}
