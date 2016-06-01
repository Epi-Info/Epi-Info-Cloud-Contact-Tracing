using System;
using System.Text;
using System.Web.Mvc;
using Epi.Core.EnterInterpreter;
namespace MvcDynamicForms.Fields
{
    [Serializable]
    public class RelateButton : InputField
    {
        new private string _promptClass = "MvcDynamicCommandButtonPrompt";
        public string RelatedViewId;       
        public RelateButton()
        {

        }
        public RelateButton(FieldAttributes fieldAttributes, double formWidth, double formHeight)
        {
            InitializeFromMetadata(fieldAttributes, formWidth, formHeight);
        }

        protected override void InitializeFromMetadata(FieldAttributes fieldAttributes, double formWidth, double formHeight)
        {
            base.InitializeFromMetadata(fieldAttributes, formWidth, formHeight);
            Title = fieldAttributes.Name;
            Prompt = fieldAttributes.PromptText;
            DisplayOrder = fieldAttributes.TabIndex;
            // Required = _FieldTypeID.Attribute("IsRequired").Value == "True" ? true : false,
            //RequiredMessage = _FieldTypeID.Attribute("PromptText").Value + " is required",
            RequiredMessage = "This field is required";
            Key = fieldAttributes.Name;
            PromptTop = formHeight * fieldAttributes.PromptTopPositionPercentage;
            PromptLeft = formWidth * fieldAttributes.PromptLeftPositionPercentage;
            Top = formHeight * fieldAttributes.ControlTopPositionPercentage;
            Left = formWidth * fieldAttributes.ControlLeftPositionPercentage;
            PromptWidth = formWidth * fieldAttributes.ControlWidthPercentage;
            ControlWidth = formWidth * fieldAttributes.ControlWidthPercentage;
            fontstyle = fieldAttributes.PromptFontStyle;
            fontSize = fieldAttributes.PromptFontSize;
            fontfamily = fieldAttributes.PromptFontFamily;
            // IsRequired = Helpers.GetRequiredControlState(form.RequiredFieldsList.ToString(), _FieldTypeID.Attribute("Name").Value, "RequiredFieldsList"),
            //Required = Helpers.GetRequiredControlState(form.RequiredFieldsList.ToString(), _FieldTypeID.Attribute("Name").Value, "RequiredFieldsList"),
            InputFieldfontstyle = fieldAttributes.ControlFontStyle;
            InputFieldfontSize = fieldAttributes.ControlFontSize;
            InputFieldfontfamily = fieldAttributes.ControlFontFamily;
            IsReadOnly = fieldAttributes.ReadOnly;
            RelatedViewId = fieldAttributes.RelatedViewId;
            //  Value = _ControlValue,
            //IsHidden = Helpers.GetControlState(SurveyAnswer, _FieldTypeID.Attribute("Name").Value, "HiddenFieldsList"),
            // IsHighlighted = Helpers.GetControlState(SurveyAnswer, _FieldTypeID.Attribute("Name").Value, "HighlightedFieldsList"),
            // IsDisabled = Helpers.GetControlState(SurveyAnswer, _FieldTypeID.Attribute("Name").Value, "DisabledFieldsList"),               

        }

        public override string RenderHtml()
        {
            string name = "mvcdynamicfield_" + _key;
            var html = new StringBuilder();
            string ErrorStyle = string.Empty;

            var commandButtonTag = new TagBuilder("button");

            //commandButtonTag.Attributes.Add("text", Prompt);
            commandButtonTag.InnerHtml = Prompt;
            commandButtonTag.Attributes.Add("id", name);
            commandButtonTag.Attributes.Add("name", "Relate");
            //commandButtonTag.Attributes.Add("name", name);
            commandButtonTag.Attributes.Add("type", "button");

            commandButtonTag.Attributes.Add("onclick", "NavigateToChild(" + RelatedViewId + ");");
            string IsHiddenStyle = "";
            string IsHighlightedStyle = "";

            if (_IsHidden)
            {
                IsHiddenStyle = "display:none";
            }

            if (_IsHighlighted)
            {
                IsHighlightedStyle = "background-color:yellow";
            }

            if (_IsDisabled)
            {
                commandButtonTag.Attributes.Add("disabled", "disabled");
            }

            // commandButtonTag.Attributes.Add("style", "position:absolute;left:" + _left.ToString() + "px;top:" + _top.ToString() + "px" + ";width:" + _Width.ToString() + "px" + ";height:" + _Height.ToString() + "px" + ErrorStyle + ";" + IsHiddenStyle + ";" + IsHighlightedStyle);
            commandButtonTag.Attributes.Add("style", "position:absolute;left:" + _left.ToString() + "px;top:" + _top.ToString() + "px" + ";width:" + ControlWidth.ToString() + "px" + ";height:" + ControlHeight.ToString() + "px" + ErrorStyle + ";" + IsHiddenStyle + ";" + IsHighlightedStyle);
            EnterRule FunctionObjectAfter = (EnterRule)_form.FormCheckCodeObj.GetCommand("level=field&event=after&identifier=" + _key);
            if (FunctionObjectAfter != null && !FunctionObjectAfter.IsNull())
            {
                commandButtonTag.Attributes.Add("onblur", "return " + _key + "_after();"); //After
            }
            EnterRule FunctionObjectBefore = (EnterRule)_form.FormCheckCodeObj.GetCommand("level=field&event=before&identifier=" + _key);
            if (FunctionObjectBefore != null && !FunctionObjectBefore.IsNull())
            {
                commandButtonTag.Attributes.Add("onfocus", "return " + _key + "_before();"); //Before
            }
            EnterRule FunctionObjectClick = (EnterRule)_form.FormCheckCodeObj.GetCommand("level=field&event=click&identifier=" + _key);
            if (FunctionObjectClick != null && !FunctionObjectClick.IsNull())
            {
                commandButtonTag.Attributes.Add("onclick", "return " + _key + "_click();");
            }

            //   html.Append(commandButtonTag.ToString(TagRenderMode.SelfClosing));
            html.Append(commandButtonTag.ToString());
            var scriptBuilder = new TagBuilder("script");
            scriptBuilder.InnerHtml = "$('#" + name + "').BlockEnter('" + name + "');";
            scriptBuilder.ToString(TagRenderMode.Normal);
            html.Append(scriptBuilder.ToString(TagRenderMode.Normal));

            var wrapper = new TagBuilder(_fieldWrapper);
            wrapper.Attributes["class"] = _fieldWrapperClass;
            if (_IsHidden)
            {
                wrapper.Attributes["style"] = "display:none";

            }
            wrapper.Attributes["id"] = name + "_fieldWrapper";
            wrapper.InnerHtml = html.ToString();
            return wrapper.ToString();
        }

        public override bool Validate()
        {
            ClearError();
            return true;
        }
        public string Value { get; set; }
        public override string Response
        {
            get { return Value; }
            set { Value = value; }
        }
    }
}
