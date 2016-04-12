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
        public FieldAttributes(XElement _FieldTypeID, XDocument SurveyAnswer, Form form)
        {
            Name = _FieldTypeID.Attribute("Name").Value;
            PromptText = _FieldTypeID.Attribute("PromptText").Value;
            TabIndex = int.Parse(_FieldTypeID.Attribute("TabIndex").Value);
            //Required = _FieldTypeID.Attribute("IsRequired").Value == "True" ? true : false,
            //RequiredMessage = _FieldTypeID.Attribute("PromptText").Value + " is required",
            //RequiredMessage = "This field is required",
            Name = _FieldTypeID.Attribute("Name").Value;
            PromptTopPositionPercentage = double.Parse(_FieldTypeID.Attribute("PromptTopPositionPercentage").Value);
            PromptLeftPositionPercentage = double.Parse(_FieldTypeID.Attribute("PromptLeftPositionPercentage").Value);
            ControlTopPositionPercentage = double.Parse(_FieldTypeID.Attribute("ControlTopPositionPercentage").Value);
            ControlLeftPositionPercentage = double.Parse(_FieldTypeID.Attribute("ControlLeftPositionPercentage").Value);
            ControlWidthPercentage = double.Parse(_FieldTypeID.Attribute("ControlWidthPercentage").Value);

            ControlHeightPercentage = double.Parse(_FieldTypeID.Attribute("ControlHeightPercentage").Value);
            PromptFontStyle = _FieldTypeID.Attribute("PromptFontStyle").Value;
            PromptFontSize = double.Parse(_FieldTypeID.Attribute("PromptFontSize").Value);
            PromptFontFamily = _FieldTypeID.Attribute("PromptFontFamily").Value;

            ControlFontStyle = _FieldTypeID.Attribute("ControlFontStyle").Value;
            ControlFontSize = double.Parse(_FieldTypeID.Attribute("ControlFontSize").Value);
            ControlFontFamily = _FieldTypeID.Attribute("ControlFontFamily").Value;

            // IsRequired = bool.Parse(_FieldTypeID.Attribute("IsRequired").Value),
            IsRequired = Helpers.GetRequiredControlState(form.RequiredFieldsList.ToString(), _FieldTypeID.Attribute("Name").Value, "RequiredFieldsList");
            Required = Helpers.GetRequiredControlState(form.RequiredFieldsList.ToString(), _FieldTypeID.Attribute("Name").Value, "RequiredFieldsList");

            ReadOnly = bool.Parse(_FieldTypeID.Attribute("IsReadOnly").Value);
            IsHidden = Helpers.GetControlState(SurveyAnswer, _FieldTypeID.Attribute("Name").Value, "HiddenFieldsList");
            IsHighlighted = Helpers.GetControlState(SurveyAnswer, _FieldTypeID.Attribute("Name").Value, "HighlightedFieldsList");
            IsDisabled = Helpers.GetControlState(SurveyAnswer, _FieldTypeID.Attribute("Name").Value, "DisabledFieldsList");

        }
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

        public bool IsRequired { get; set; }
        public bool Required { get; set; }

        public bool ReadOnly { get; set; }
        public bool IsHidden { get; set; }
        public bool IsHighlighted { get; set; }
        public bool IsDisabled { get; set; }


    }
}