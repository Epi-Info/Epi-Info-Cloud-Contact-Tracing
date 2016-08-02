using System.Linq;

namespace Epi.Cloud.Common.Metadata
{
    public class AbridgedFieldInfo
    {
        public AbridgedFieldInfo(Field field)
        {
            Name = field.Name;
            FieldType = field.FieldTypeId;
            IsReadonly = Epi.Cloud.Common.Metadata.FieldType.ReadonlyFieldTypes.Contains(field.FieldTypeId);
        }

        public string Name { get; set; }
        public int FieldType { get; set; }
        public bool IsReadonly { get; set; }
    }
}
