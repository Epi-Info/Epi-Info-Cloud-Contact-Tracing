using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace MvcDynamicForms.Fields
{
    public class FieldAttributes
    {
        int _tempInt;
        double _tempDouble;
        bool _tempBool;

        public FieldAttributes()
        {
        }

        public FieldAttributes(XElement fieldType, XDocument surveyAnswer, Form form)
        {
            RequiredMessage = "This field is required";
            UniqueId = fieldType.AttributeValue("UniqueId");
            FieldTypeId = int.TryParse(fieldType.AttributeValue("FieldTypeId"), out _tempInt) ? _tempInt : 0;
            Name = fieldType.AttributeValue("Name");
            TabIndex = int.TryParse(fieldType.AttributeValue("TabIndex"), out _tempInt) ? _tempInt : 0;

            PromptText = fieldType.AttributeValue("PromptText").Trim();
            PromptTopPositionPercentage = double.TryParse(fieldType.AttributeValue("PromptTopPositionPercentage"), out _tempDouble) ? _tempDouble : 0;
            PromptLeftPositionPercentage = double.TryParse(fieldType.AttributeValue("PromptLeftPositionPercentage"), out _tempDouble) ? _tempDouble : 0;
            PromptFontStyle = fieldType.AttributeValue("PromptFontStyle");
            PromptFontSize = double.TryParse(fieldType.AttributeValue("PromptFontSize"), out _tempDouble) ? _tempDouble : 0;
            PromptFontFamily = fieldType.AttributeValue("PromptFontFamily");

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

            IsRequired = Helpers.GetRequiredControlState(form.RequiredFieldsList.ToString(), fieldType.AttributeValue("Name"), "RequiredFieldsList");
            Required = Helpers.GetRequiredControlState(form.RequiredFieldsList.ToString(), fieldType.AttributeValue("Name"), "RequiredFieldsList");
            ReadOnly = bool.TryParse(fieldType.AttributeValue("IsReadOnly"), out _tempBool) ? _tempBool : false;
            IsHidden = Helpers.GetControlState(surveyAnswer, fieldType.AttributeValue("Name"), "HiddenFieldsList");
            IsHighlighted = Helpers.GetControlState(surveyAnswer, fieldType.AttributeValue("Name"), "HighlightedFieldsList");
            IsDisabled = Helpers.GetControlState(surveyAnswer, fieldType.AttributeValue("Name"), "DisabledFieldsList");
            ShowTextOnRight = fieldType.AttributeValue("ShowTextOnRight");
            ChoicesList = fieldType.AttributeValue("List");
            BackgroundColor = fieldType.AttributeValue("BackgroundColor");
        }

        public string UniqueId { get; set; }
        public int FieldTypeId { get; set; }
        public string RequiredMessage { get; set; }
        public string Name { get; set; }
        public int TabIndex { get; set; }

        public string PromptText { get; set; }
        public double PromptTopPositionPercentage { get; set; }
        public double PromptLeftPositionPercentage { get; set; }

        public double ControlTopPositionPercentage { get; set; }
        public double ControlLeftPositionPercentage { get; set; }
        public double ControlWidthPercentage { get; set; }
        public double ControlHeightPercentage { get; set; }

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
        public bool ReadOnly { get; set; }
        public bool IsHidden { get; set; }
        public bool IsHighlighted { get; set; }
        public bool IsDisabled { get; set; }

        public string ShowTextOnRight { get; set; }
        public string ChoicesList { get; set; }
        public string BackgroundColor { get; set; }
    }

    public static class XmlAttributeExtensions
    {
        public static string AttributeValue(this XElement fieldType, XName attrName, string defaultValue = null)
        {
            var attribute = fieldType.Attribute(attrName);
            return attribute != null ? attribute.Value : defaultValue;
        }
    }
}