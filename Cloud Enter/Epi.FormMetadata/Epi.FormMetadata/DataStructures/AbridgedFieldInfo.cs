using System.Linq;
using Epi.FormMetadata.Constants;
using Epi.FormMetadata.DataStructures.Interfaces;
using Epi.FormMetadata.Utilities;
using Newtonsoft.Json;

namespace Epi.FormMetadata.DataStructures
{    public class AbridgedFieldInfo : IAbridgedFieldInfo
    {
 
        protected MutableAttributes _mutableAttributes;

        public AbridgedFieldInfo()
        {
            _mutableAttributes = new MutableAttributes();
        }

        public AbridgedFieldInfo(IAbridgedFieldInfo fieldAttributes) : this()
        {
            if (fieldAttributes != null)
            {
                FieldName = fieldAttributes.FieldName;
				TrueCaseFieldName = fieldAttributes.TrueCaseFieldName;
                FieldType = (FieldTypes)fieldAttributes.FieldType;
                List = fieldAttributes.List;
                IsReadOnly = FieldMetadata.ReadonlyFieldTypes.Contains((int)fieldAttributes.FieldType);
                IsRequired = fieldAttributes.IsRequired;
            }
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
        public bool IsRequired { get { return _mutableAttributes.IsRequired; } set { _mutableAttributes.IsRequired = value; } }
        public bool IsHidden { get { return _mutableAttributes.IsHidden; } set { _mutableAttributes.IsHidden = value; } }
        public bool IsDisabled { get { return _mutableAttributes.IsDisabled; } set { _mutableAttributes.IsDisabled = value; } }
        public bool IsHighlighted { get { return _mutableAttributes.IsHighlighted; } set { _mutableAttributes.IsHighlighted = value; } }
        public string Value { get { return _mutableAttributes.Value; } set { _mutableAttributes.Value = value; } }
        public FieldDataType DataType { get { return FieldMetadata.GetDataType(FieldType); } }

        [JsonProperty]
        public MutableAttributes MutableAttributes { get { return _mutableAttributes; } protected set { _mutableAttributes = value; } }

        public AbridgedFieldInfo Clone()
        {
            var clone = (AbridgedFieldInfo)MemberwiseClone();
            clone._mutableAttributes = new MutableAttributes
            {
                IsRequired = this.IsRequired,
                IsHidden = this.IsHidden,
                IsDisabled = this.IsDisabled,
                IsHighlighted = this.IsHighlighted,
                Value = this.Value
            };
            return clone;
        }
    }

    
    public class MutableAttributes
    {
        public bool IsRequired { get; set; }
        public bool IsHidden { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsHighlighted { get; set; }
        public string Value { get; set; }

        public MutableAttributes Clone()
        {
            var clone = (MutableAttributes)MemberwiseClone();
            return clone;
        }
    }


}
