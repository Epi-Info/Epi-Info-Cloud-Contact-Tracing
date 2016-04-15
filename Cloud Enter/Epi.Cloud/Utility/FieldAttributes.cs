using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using MvcDynamicForms;

namespace Epi.Web.MVC.Utility
{
    class FieldAttributes
    {
        int _tempInt;
        double _tempDouble;

        public FieldAttributes(XElement _FieldTypeID, XDocument SurveyAnswer, Form form)
        {
            RequiredMessage = "This field is required";

            Name = _FieldTypeID.Attribute("Name").Value;
            PromptText = _FieldTypeID.Attribute("PromptText").Value.Trim();
            TabIndex = int.TryParse(_FieldTypeID.Attribute("TabIndex").Value, out _tempInt) ? _tempInt : 0;
            Name = _FieldTypeID.Attribute("Name").Value;
            PromptTopPositionPercentage = double.TryParse(_FieldTypeID.Attribute("PromptTopPositionPercentage").Value, out _tempDouble) ? _tempDouble : 0;
            PromptLeftPositionPercentage = double.TryParse(_FieldTypeID.Attribute("PromptLeftPositionPercentage").Value, out _tempDouble) ? _tempDouble : 0;
            ControlTopPositionPercentage = double.TryParse(_FieldTypeID.Attribute("ControlTopPositionPercentage").Value, out _tempDouble) ? _tempDouble : 0;
            ControlLeftPositionPercentage = double.TryParse(_FieldTypeID.Attribute("ControlLeftPositionPercentage").Value, out _tempDouble) ? _tempDouble : 0;
            ControlWidthPercentage = double.TryParse(_FieldTypeID.Attribute("ControlWidthPercentage").Value, out _tempDouble) ? _tempDouble : 0;
            ControlHeightPercentage = double.TryParse(_FieldTypeID.Attribute("ControlHeightPercentage").Value, out _tempDouble) ? _tempDouble : 0;
            PromptFontStyle = _FieldTypeID.Attribute("PromptFontStyle").Value;
            PromptFontSize = double.TryParse(_FieldTypeID.Attribute("PromptFontSize").Value, out _tempDouble) ? _tempDouble : 0;
            PromptFontFamily = _FieldTypeID.Attribute("PromptFontFamily").Value;

            ControlFontStyle = _FieldTypeID.Attribute("ControlFontStyle").Value;
            ControlFontSize = double.TryParse(_FieldTypeID.Attribute("ControlFontSize").Value, out _tempDouble) ? _tempDouble : 0;
            ControlFontFamily = _FieldTypeID.Attribute("ControlFontFamily").Value;

            MaxLength = int.TryParse(_FieldTypeID.Attribute("MaxLength").Value, out _tempInt) ? _tempInt : 0;

            IsRequired = Helpers.GetRequiredControlState(form.RequiredFieldsList.ToString(), _FieldTypeID.Attribute("Name").Value, "RequiredFieldsList");
            Required = Helpers.GetRequiredControlState(form.RequiredFieldsList.ToString(), _FieldTypeID.Attribute("Name").Value, "RequiredFieldsList");

            ReadOnly = bool.Parse(_FieldTypeID.Attribute("IsReadOnly").Value);
            IsHidden = Helpers.GetControlState(SurveyAnswer, _FieldTypeID.Attribute("Name").Value, "HiddenFieldsList");
            IsHighlighted = Helpers.GetControlState(SurveyAnswer, _FieldTypeID.Attribute("Name").Value, "HighlightedFieldsList");
            IsDisabled = Helpers.GetControlState(SurveyAnswer, _FieldTypeID.Attribute("Name").Value, "DisabledFieldsList");
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

        public bool IsRequired { get; set; }
        public bool Required { get; set; }

        public bool ReadOnly { get; set; }
        public bool IsHidden { get; set; }
        public bool IsHighlighted { get; set; }
        public bool IsDisabled { get; set; }


    }
}