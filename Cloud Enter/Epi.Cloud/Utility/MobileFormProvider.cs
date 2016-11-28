using MvcDynamicForms;
using MvcDynamicForms.Fields;
using Epi.Cloud.Common.Metadata;

namespace Epi.Web.MVC.Utility
{
    public class MobileFormProvider : FormProvider
    {
        public bool isAndroid = false;
        public MobileFormProvider()
		{
		}

		public MobileFormProvider(string formId) : base(formId)
		{
		}

		protected override void SetProviderSpecificProperties(Form form, double height, double width)
        {
            form.FormWrapperClass = "MvcDynamicMobileForm";
            form.IsMobile = true;
            isAndroid = form.IsAndroid;
        }

        protected override void AddRadioButtonGroupBox(Form form, FieldAttributes fieldAttributes, double _Width, double _Height)
        {
            // Don't add a group box around radio buttons for mobile devices
        }

        protected override MvcDynamicForms.Fields.Field GetRadioList(FieldAttributes fieldAttributes, double formWidth, double formHeight, string controlValue)
        {
            var radiolist = new MobileRadioList(fieldAttributes, formWidth, formHeight)
            {
                Value = controlValue
            };

            return radiolist;
        }

        protected override MvcDynamicForms.Fields.Field GetNumericTextBox(FieldAttributes fieldAttributes, double formWidth, double formHeight, string controlValue)
        {
            var numericTextBox = new MobileNumericTextBox(fieldAttributes, formWidth, formHeight)
            {
                Value = controlValue
            };

            return numericTextBox;
        }

        protected override MvcDynamicForms.Fields.Field GetLabel(FieldAttributes fieldAttributes, double formWidth, double formHeight)
        {
            var label = new MobileLiteral(fieldAttributes, formWidth, formHeight);
            return label;
        }

        protected override MvcDynamicForms.Fields.Field GetTextArea(FieldAttributes fieldAttributes, double formWidth, double formHeight, string controlValue)
        {
            var textArea = new MobileTextArea(fieldAttributes, formWidth, formHeight)
            {
                Value = controlValue
            };
            return textArea;
        }

        protected override MvcDynamicForms.Fields.Field GetTextBox(FieldAttributes fieldAttributes, double formWidth, double formHeight, string controlValue)
        {
            var textBox = new MobileTextBox(fieldAttributes, formWidth, formHeight)
            {
                Value = controlValue
            };

            return textBox;
        }

        protected override MvcDynamicForms.Fields.Field GetCheckBox(FieldAttributes fieldAttributes, double formWidth, double formHeight, string controlValue)
        {
            var checkBox = new MobileCheckBox(fieldAttributes, formWidth, formHeight)// CheckBox (fieldAttributes, formWidth, formHeight)
            {
                Value = controlValue,
                Response = controlValue
            };

            return checkBox;
        }

        protected override MvcDynamicForms.Fields.Field GetDatePicker(FieldAttributes fieldAttributes, double formWidth, double formHeight, string controlValue)
        {
            var DatePicker = new MobileDatePicker(fieldAttributes, formWidth, formHeight)
            {
                Value = controlValue,
                Response = controlValue
            };

            return DatePicker;
        }


        protected override MvcDynamicForms.Fields.Field GetTimePicker(FieldAttributes fieldAttributes, double formWidth, double formHeight, string controlValue)
        {
            var TimePicker = new MobileTimePicker(fieldAttributes, formWidth, formHeight)
            {
                Value = controlValue,
                Response = controlValue
            };

            return TimePicker;
        }

        protected override MvcDynamicForms.Fields.Field GetRelateButton(FieldAttributes fieldAttributes, double formWidth, double formHeight)
        {
            var RelateButton = new MobileRelateButton(fieldAttributes, formWidth, formHeight)
            {
                // Don't set the control's value for mobile devices
                // Value = controlValue
            };

            return RelateButton;
        }

        protected override MvcDynamicForms.Fields.Field GetDropDown(FieldAttributes fieldAttributes, double formWidth, double formHeight, string controlValue, string DropDownValues, int FieldTypeId)
        {
            var select = new MobileSelect(fieldAttributes, formWidth, formHeight)//, DropDownValues, FieldTypeId)
            {
                Value = controlValue
            };
            select.SelectType = FieldTypeId;
            select.SelectedValue = controlValue;

            select.ShowEmptyOption = true;
            select.EmptyOption = "Select";
            select.AddChoices(DropDownValues, "&#;");
            select.SelectedValue = controlValue;
            if (!string.IsNullOrWhiteSpace(controlValue))
            {
                select.Choices[controlValue] = true;
            }

            return select;
        }

        protected override MvcDynamicForms.Fields.Field GetGroupBox(FieldAttributes fieldAttributes, double formWidth, double formHeight, string controlValue)
        {
            var groupbox = new MobileGroupBox(fieldAttributes, formWidth, formHeight)
            {
                // Don't set the control's value for mobile devices
                //  Value = controlValue
            };

            return groupbox;
        }
    }
}
