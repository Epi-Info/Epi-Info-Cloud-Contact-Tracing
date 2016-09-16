using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Epi.Cloud.Common.Metadata.Interfaces;
using Newtonsoft.Json;

namespace Epi.Cloud.Common.Metadata
{
    public class FieldAttributes : IAbridgedFieldInfo
    {
        [JsonIgnore]
        int _tempInt;
        [JsonIgnore]
        double _tempDouble;
        [JsonIgnore]
        bool _tempBool;       

        public FieldAttributes()
        {
        }

        public FieldAttributes(XElement fieldType, XDocument surveyAnswer, string requiredFieldsList)
        {
            RequiredMessage = "This field is required";
            UniqueId = fieldType.AttributeValue("UniqueId");
            FieldType = (FieldTypes) (int.TryParse(fieldType.AttributeValue("FieldTypeId"), out _tempInt) ? _tempInt : 0);
            FieldName = fieldType.AttributeValue("Name");
            TabIndex = int.TryParse(fieldType.AttributeValue("TabIndex"), out _tempInt) ? _tempInt : 0;

            PromptText = fieldType.AttributeValue("PromptText").Trim();
            PromptTopPositionPercentage = double.TryParse(fieldType.AttributeValue("PromptTopPositionPercentage"), out _tempDouble) ? _tempDouble : 0;
            PromptLeftPositionPercentage = double.TryParse(fieldType.AttributeValue("PromptLeftPositionPercentage"), out _tempDouble) ? _tempDouble : 0;
            PromptFontStyle = fieldType.AttributeValue("PromptFontStyle");
            PromptFontSize = double.TryParse(fieldType.AttributeValue("PromptFontSize"), out _tempDouble) ? _tempDouble : 0;
            PromptFontFamily = fieldType.AttributeValue("PromptFontFamily");
            Checkcode = fieldType.AttributeValue("checkcode");
            ControlTopPositionPercentage = double.TryParse(fieldType.AttributeValue("ControlTopPositionPercentage"), out _tempDouble) ? _tempDouble : 0;
            ControlLeftPositionPercentage = double.TryParse(fieldType.AttributeValue("ControlLeftPositionPercentage"), out _tempDouble) ? _tempDouble : 0;
            ControlWidthPercentage = double.TryParse(fieldType.AttributeValue("ControlWidthPercentage"), out _tempDouble) ? _tempDouble : 0;
            ControlHeightPercentage = double.TryParse(fieldType.AttributeValue("ControlHeightPercentage"), out _tempDouble) ? _tempDouble : 0;
            ControlFontStyle = fieldType.AttributeValue("ControlFontStyle");
            ControlFontSize = double.TryParse(fieldType.AttributeValue("ControlFontSize"), out _tempDouble) ? _tempDouble : 0;
            ControlFontFamily = fieldType.AttributeValue("ControlFontFamily");

            MaxLength = int.TryParse(fieldType.AttributeValue("MaxLength"), out _tempInt) ? _tempInt : 0;
            Pattern = fieldType.AttributeValue("Pattern");
            Lower = fieldType.AttributeValue("Lower");
            Upper = fieldType.AttributeValue("Upper");

            IsRequired = Helpers.GetRequiredControlState(requiredFieldsList, fieldType.AttributeValue("Name"), "RequiredFieldsList");
            Required = Helpers.GetRequiredControlState(requiredFieldsList, fieldType.AttributeValue("Name"), "RequiredFieldsList");
            IsReadOnly = bool.TryParse(fieldType.AttributeValue("IsReadOnly"), out _tempBool) ? _tempBool : false;
            IsHidden = Helpers.GetControlState(surveyAnswer, fieldType.AttributeValue("Name"), "HiddenFieldsList");
            IsHighlighted = Helpers.GetControlState(surveyAnswer, fieldType.AttributeValue("Name"), "HighlightedFieldsList");
            IsDisabled = Helpers.GetControlState(surveyAnswer, fieldType.AttributeValue("Name"), "DisabledFieldsList");
            ShowTextOnRight = fieldType.AttributeValue("ShowTextOnRight");
            List = fieldType.AttributeValue("List");
            BackgroundColor = fieldType.AttributeValue("BackgroundColor");
            RelatedViewId = fieldType.AttributeValue("RelatedViewId");
        }

        public int ViewId { get; set; }
        public int PageId { get; set; }
        public int PagePosition { get; set; }
        public string PageName { get; set; }

        public string UniqueId { get; set; }
        public FieldTypes FieldType { get; set; }
        public string RequiredMessage { get; set; }
        public string FieldName { get; set; }
        public int TabIndex { get; set; }

        public string PromptText { get; set; }
        public double PromptTopPositionPercentage { get; set; }
        public double PromptLeftPositionPercentage { get; set; }

        public double ControlTopPositionPercentage { get; set; }
        public double ControlLeftPositionPercentage { get; set; }
        public double ControlWidthPercentage { get; set; }
        public double ControlHeightPercentage { get; set; }
        public string Checkcode { get; set; }
        public double PromptFontSize { get; set; }
        public string PromptFontStyle { get; set; }
        public string PromptFontFamily { get; set; }

        public double ControlFontSize { get; set; }
        public string ControlFontStyle { get; set; }
        public string ControlFontFamily { get; set; }

        public int MaxLength { get; set; }
        public string Pattern { get; set; }
        public string Lower { get; set; }
        public string Upper { get; set; }

        public bool IsRequired { get; set; }
        public bool Required { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsHidden { get; set; }
        public bool IsHighlighted { get; set; }
        public bool IsDisabled { get; set; }

        public string ShowTextOnRight { get; set; }
        public string List { get; set; }
        public string BackgroundColor { get; set; }
        public string[] SourceTableValues { get; set; }
        public string RelatedViewId { get; set; }


        public static IEnumerable<FieldAttributes> MapFieldMetadataToFieldAttributes(Page page, string formCheckcode)
        {
            var fields = page.Fields;
            return MapFieldMetadataToFieldAttributes(fields, formCheckcode);
        }

        public static IEnumerable<FieldAttributes> MapFieldMetadataToFieldAttributes(Common.Metadata.Field[] fields, string formCheckcode)
        {
            var sourceTableFields = fields.Where(f => f.SourceTableValues != null).ToArray();
            var results = fields.Select(f => new FieldAttributes
            {
                RequiredMessage = "This field is required",

                ViewId = f.ViewId,
                PageId = f.PageId.ValueOrDefault(),
                PageName = f.PageName,
                PagePosition = f.PagePosition.ValueOrDefault(),
                Checkcode = formCheckcode,
                UniqueId = f.UniqueId.ToString("D"),
                FieldType = (FieldTypes)f.FieldTypeId,
                FieldName = f.Name,
                TabIndex = (int)f.TabIndex,

                PromptText = f.PromptText,
                PromptTopPositionPercentage = f.PromptTopPositionPercentage.ValueOrDefault(),
                PromptLeftPositionPercentage = f.PromptLeftPositionPercentage.ValueOrDefault(),
                PromptFontStyle = f.PromptFontStyle,
                PromptFontSize = (double)f.PromptFontSize.ValueOrDefault(),
                PromptFontFamily = f.PromptFontFamily,

                ControlTopPositionPercentage = f.ControlTopPositionPercentage.ValueOrDefault(),
                ControlLeftPositionPercentage = f.ControlLeftPositionPercentage.ValueOrDefault(),
                ControlWidthPercentage = f.ControlWidthPercentage.ValueOrDefault(),
                ControlHeightPercentage = f.ControlHeightPercentage.ValueOrDefault(),
                ControlFontStyle = f.ControlFontStyle,
                ControlFontSize = (double)f.ControlFontSize.ValueOrDefault(),
                ControlFontFamily = f.ControlFontFamily,

                MaxLength = f.MaxLength.ValueOrDefault(),
                Pattern = f.Pattern,
                Lower = f.Lower,
                Upper = f.Upper,
                IsRequired =  f.IsRequired?? false,
                Required = f.IsRequired?? false,
                IsReadOnly = f.IsReadOnly ?? false,
                IsHidden = false,
                IsHighlighted = false,
                IsDisabled = false,
                List = f.List,
                SourceTableValues = f.SourceTableValues,
                RelatedViewId = f.RelatedViewId.ToString()

            });
            return results;
        }
    }


}