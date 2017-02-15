using System;
using System.Collections.Generic;
using System.Text;
using Epi.Cloud.Common.Metadata;

namespace MvcDynamicForms.Fields
{
    /// <summary>
    /// Represents a dynamically generated html input field.
    /// </summary>
    [Serializable]
    public abstract class InputField : Field
    {
        protected double _promptTop;
        protected double _promptLeft;
        protected double _promptWidth;
        protected double _controlWidth;
        protected double _controlHeight;
        protected string _key = Guid.NewGuid().ToString();
        protected string _requiredMessage = "Required";
        protected string _promptClass = "MvcDynamicFieldPrompt";
        protected string _errorClass = "MvcDynamicFieldError";
        protected Boolean _isRequired;
        protected Boolean _isReadOnly;
        protected int _MaxLength;
        protected string _InputFieldfontstyle;
        protected double _InputFieldfontSize;
        protected string _InputFieldfontfamily;
        protected string _BackgroundColor;
        protected int _FieldTypeId;
        protected Dictionary<string, string> _inputHtmlAttributes = new Dictionary<string, string>();

        /// <summary>
        /// Used to identify each InputField when performing model binding.
        /// </summary>
        /// 
        public int FieldTypeId { get; set; }

        public string Key
        {
            get { return _key; }
            set { _key = value.ToLower(); }
        }
        /// <summary>
        /// Used to identify InputFields when working with end users' responses.
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// The question asked of the end user. This text appears before the html input element.
        /// </summary>
        public string Prompt { get; set; }
        /// <summary>
        /// The html class applied to the label element that appears before the input element.
        /// </summary>
        public string PromptClass
        {
            get
            {
                return _promptClass;
            }
            set
            {
                _promptClass = value;
            }
        }
        public string Value { get; set; }
        /// <summary>
        /// String representing the user's response to the field.
        /// </summary>
        public abstract string Response { get; set; }
        /// <summary>
        /// Whether the field must be completed to be valid.
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Whether the field is readonly. If the field is readonly then the required = false
        /// </summary>
        public bool ReadOnly { get; set; }
        /// <summary>
        /// The error message that the end user sees if they do not complete the field.
        /// </summary>
        public string RequiredMessage
        {
            get
            {
                return _requiredMessage;
            }
            set
            {
                _requiredMessage = value;
            }
        }
        /// <summary>
        /// The error message that the end user sees.
        /// </summary>
        public string Error { get; set; }
        /// <summary>
        /// The class attribute of the label element that is used to display an error message to the user.
        /// </summary>
        public string ErrorClass
        {
            get
            {
                return _errorClass;
            }
            set
            {
                _errorClass = value;
            }
        }
        /// <summary>
        /// True if the field is valid; false otherwise.
        /// </summary>
        public bool IsValid
        {
            get
            {
                return string.IsNullOrEmpty(Error);
            }
        }
        /// <summary>
        /// Collection of html attribute names and values that will be applied to the rendered input elements.
        /// </summary>
        public Dictionary<string, string> InputHtmlAttributes
        {
            get
            {
                return _inputHtmlAttributes;
            }
            set
            {
                _inputHtmlAttributes = value;
            }
        }

        protected override void InitializeFromMetadata(FieldAttributes fieldAttributes, double formWidth, double formHeight)
        {
            base.InitializeFromMetadata(fieldAttributes, formWidth, formHeight);

            Title = fieldAttributes.FieldName;
            Prompt = fieldAttributes.PromptText;
            Key = fieldAttributes.FieldName;
            PromptTop = formHeight * fieldAttributes.PromptTopPositionPercentage;
            PromptLeft = formWidth * fieldAttributes.PromptLeftPositionPercentage;
            PromptWidth = formWidth * fieldAttributes.ControlWidthPercentage;
            ControlWidth = formWidth * fieldAttributes.ControlWidthPercentage;
            ControlHeight = formHeight * fieldAttributes.ControlHeightPercentage;

            InputFieldfontstyle = fieldAttributes.ControlFontStyle;
            InputFieldfontfamily = fieldAttributes.ControlFontFamily;
            InputFieldfontSize = fieldAttributes.ControlFontSize;

            IsRequired = fieldAttributes.IsRequired;
            Required = fieldAttributes.Required;
            RequiredMessage = fieldAttributes.RequiredMessage;
            ReadOnly = fieldAttributes.IsReadOnly;
        }

        /// <summary>
        /// Validates the user's response.
        /// </summary>
        /// <returns></returns>
        public abstract bool Validate();
        /// <summary>
        /// Removes the message stored in the Error property.
        /// </summary>
        public void ClearError()
        {
            Error = null;
        }

        public double PromptTop { get { return this._promptTop; } set { this._promptTop = value; } }
        public double PromptLeft { get { return this._promptLeft; } set { this._promptLeft = value; } }

        public double PromptWidth { get { return this._promptWidth; } set { this._promptWidth = value; } }
        public double ControlWidth { get { return System.Math.Truncate(this._controlWidth); } set { this._controlWidth = System.Math.Truncate(value); } }

        public double ControlHeight { get { return System.Math.Truncate(this._controlHeight); } set { this._controlHeight = System.Math.Truncate(value); } }
        public Boolean IsRequired { get { return this._isRequired; } set { this._isRequired = value; } }
        public Boolean IsReadOnly { get { return this._isReadOnly; } set { this._isReadOnly = value; } }
        public int MaxLength { get { return this._MaxLength; } set { this._MaxLength = value; } }

        //  protected string InputFieldfontstyle;
        //  protected string InputFieldfontSize;
        // protected string InputFieldfontfamily;
        public string InputFieldfontstyle { get { return this._InputFieldfontstyle; } set { this._InputFieldfontstyle = value; } }
        public double InputFieldfontSize { get { return this._InputFieldfontSize; } set { this._InputFieldfontSize = value; } }
        public string InputFieldfontfamily { get { return this._InputFieldfontfamily; } set { this._InputFieldfontfamily = value; } }
        public string BackgroundColor { get { return this._BackgroundColor; } set { this._BackgroundColor = value; } }

        public string GetInputFieldStyle(string ControlFontStyle, double ControlFontSize, string ControlFontFamily)
        {

            StringBuilder FontStyle = new StringBuilder();
            StringBuilder FontWeight = new StringBuilder();
            StringBuilder TextDecoration = new StringBuilder();
            StringBuilder CssStyles = new StringBuilder();

            char[] delimiterChars = { ' ', ',' };
            string[] Styles = ControlFontStyle.Split(delimiterChars);


            foreach (string Style in Styles)
            {
                switch (Style.ToString())
                {
                    case "Italic":
                        FontStyle.Append(Style.ToString());
                        break;
                    case "Oblique":
                        FontStyle.Append(Style.ToString());

                        break;

                }

            }
            foreach (string Style in Styles)
            {
                switch (Style.ToString())
                {
                    case "Bold":
                        FontWeight.Append(Style.ToString());
                        break;
                    case "Normal":
                        FontWeight.Append(Style.ToString());
                        break;


                }

            }
            CssStyles.Append(";font:");//1
            if (!string.IsNullOrEmpty(FontStyle.ToString()))
            {

                CssStyles.Append(FontStyle);//2
                CssStyles.Append(" ");//3
            }
            CssStyles.Append(FontWeight);
            CssStyles.Append(" ");
            CssStyles.Append(this._InputFieldfontSize.ToString() + "pt ");
            CssStyles.Append(" ");
            CssStyles.Append(this._InputFieldfontfamily.ToString());

            foreach (string Style in Styles)
            {
                switch (Style.ToString())
                {
                    case "Strikeout":
                        TextDecoration.Append("line-through");
                        break;
                    case "Underline":
                        TextDecoration.Append(Style.ToString());

                        break;

                }

            }

            if (!string.IsNullOrEmpty(TextDecoration.ToString()))
            {
                CssStyles.Append(";text-decoration:");
            }

            CssStyles.Append(TextDecoration);


            return CssStyles.ToString();

        }
    }
}
