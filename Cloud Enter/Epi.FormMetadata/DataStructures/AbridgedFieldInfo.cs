using System.Linq;
using Epi.FormMetadata.Constants;
using Epi.FormMetadata.DataStructures.Interfaces;
using Epi.FormMetadata.Utilities;
using Newtonsoft.Json;

namespace Epi.FormMetadata.DataStructures
{
    public class AbridgedFieldInfo : IAbridgedFieldInfo
    {
 
        public AbridgedFieldInfo()
        {
        }

        public AbridgedFieldInfo(Field field) : this()
        {
            if (field != null)
            {
                FieldName = field.Name.ToLower();
				TrueCaseFieldName = field.Name;
				FieldType = (FieldTypes)field.FieldTypeId;
                List = field.List;
                IsReadOnly = FieldMetadata.ReadonlyFieldTypes.Contains(field.FieldTypeId) || (field.IsReadOnly.HasValue ? field.IsReadOnly.Value : false);
                IsRequired = field.IsRequired.HasValue ? field.IsRequired.Value : false;
            }
        }

        [JsonProperty]
        public string FieldName { get; protected set; }

		[JsonProperty]
		public string TrueCaseFieldName { get; protected set; }

        [JsonProperty]
        public FieldTypes FieldType { get; protected set; }

        [JsonProperty]
        public string List { get; protected set; }

        [JsonProperty]
        public bool IsReadOnly { get; protected set; }

        [JsonProperty]
        public bool IsRequired { get; protected set; }

        [JsonProperty]
        public bool IsHidden { get; protected set; }

        [JsonProperty]
        public bool IsDisabled { get; protected set; }

        [JsonProperty]
        public bool IsHighlighted { get; protected set; }

        [JsonIgnore]
        public FieldDataType DataType { get { return FieldMetadata.GetDataType(FieldType); } }
    }
}
