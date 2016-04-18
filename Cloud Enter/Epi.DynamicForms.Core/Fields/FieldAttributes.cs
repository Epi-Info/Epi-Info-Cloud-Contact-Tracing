using System;
using System.Xml.Linq;

namespace MvcDynamicForms.Fields
{
    public class FieldAttributes
    {
        int _tempInt;
        double _tempDouble;

        public FieldAttributes(XElement fieldType, XDocument surveyAnswer, Form form)
        {
            RequiredMessage = "This field is required";

            Name = fieldType.Attribute("Name").Value;
            PromptText = fieldType.Attribute("PromptText").Value.Trim();
            TabIndex = int.TryParse(fieldType.Attribute("TabIndex").Value, out _tempInt) ? _tempInt : 0;
            Name = fieldType.Attribute("Name").Value;
            PromptTopPositionPercentage = double.TryParse(fieldType.Attribute("PromptTopPositionPercentage").Value, out _tempDouble) ? _tempDouble : 0;
            PromptLeftPositionPercentage = double.TryParse(fieldType.Attribute("PromptLeftPositionPercentage").Value, out _tempDouble) ? _tempDouble : 0;
            ControlTopPositionPercentage = double.TryParse(fieldType.Attribute("ControlTopPositionPercentage").Value, out _tempDouble) ? _tempDouble : 0;
            ControlLeftPositionPercentage = double.TryParse(fieldType.Attribute("ControlLeftPositionPercentage").Value, out _tempDouble) ? _tempDouble : 0;
            ControlWidthPercentage = double.TryParse(fieldType.Attribute("ControlWidthPercentage").Value, out _tempDouble) ? _tempDouble : 0;
            ControlHeightPercentage = double.TryParse(fieldType.Attribute("ControlHeightPercentage").Value, out _tempDouble) ? _tempDouble : 0;
            PromptFontStyle = fieldType.Attribute("PromptFontStyle").Value;
            PromptFontSize = double.TryParse(fieldType.Attribute("PromptFontSize").Value, out _tempDouble) ? _tempDouble : 0;
            PromptFontFamily = fieldType.Attribute("PromptFontFamily").Value;

            ControlFontStyle = fieldType.Attribute("ControlFontStyle").Value;
            ControlFontSize = double.TryParse(fieldType.Attribute("ControlFontSize").Value, out _tempDouble) ? _tempDouble : 0;
            ControlFontFamily = fieldType.Attribute("ControlFontFamily").Value;

            MaxLength = int.TryParse(fieldType.Attribute("MaxLength").Value, out _tempInt) ? _tempInt : 0;
            Pattern = fieldType.Attribute("Pattern").Value;
            Lower = fieldType.Attribute("Lower").Value;
            Upper = fieldType.Attribute("Upper").Value;

            IsRequired = Helpers.GetRequiredControlState(form.RequiredFieldsList.ToString(), fieldType.Attribute("Name").Value, "RequiredFieldsList");
            Required = Helpers.GetRequiredControlState(form.RequiredFieldsList.ToString(), fieldType.Attribute("Name").Value, "RequiredFieldsList");

            ReadOnly = bool.Parse(fieldType.Attribute("IsReadOnly").Value);
            IsHidden = Helpers.GetControlState(surveyAnswer, fieldType.Attribute("Name").Value, "HiddenFieldsList");
            IsHighlighted = Helpers.GetControlState(surveyAnswer, fieldType.Attribute("Name").Value, "HighlightedFieldsList");
            IsDisabled = Helpers.GetControlState(surveyAnswer, fieldType.Attribute("Name").Value, "DisabledFieldsList");

        }

        public string RequiredMessage { get; set; }
        public String Name { get; set; }
        public String PromptText { get; set; }
        public int TabIndex { get; set; }
        public double PromptTopPositionPercentage { get; set; }
        public double PromptLeftPositionPercentage { get; set; }
        public double ControlTopPositionPercentage { get; set; }
        public double ControlLeftPositionPercentage { get; set; }

        public double ControlWidthPercentage { get; set; }

        public double ControlHeightPercentage { get; set; }

        public double PromptFontSize { get; set; }
        public String PromptFontStyle { get; set; }
        public String PromptFontFamily { get; set; }

        public double ControlFontSize { get; set; }
        public String ControlFontStyle { get; set; }
        public String ControlFontFamily { get; set; }

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
    }
}