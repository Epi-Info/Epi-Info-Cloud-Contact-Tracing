using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using Epi.Cloud.Common.Metadata;

namespace MvcDynamicForms.Fields
{
    /// <summary>
    /// Represents html to be rendered on the form.
    /// </summary>
    [Serializable]
    public class Literal : Field
    {
        public Literal(FieldAttributes fieldAttributes, double formWidth, double formHeight)
        {
            InitializeFromMetadata(fieldAttributes, formWidth, formHeight);
        }

        public string Name { get; set; }

        /// <summary>
        /// Determines whether the rendered html will be wrapped by another element.
        /// </summary>
        public bool Wrap { get; set; }

        /// <summary>
        /// The html to be rendered on the form.
        /// </summary>
        /// 
        public string Html { get; set; }

        protected override void InitializeFromMetadata(FieldAttributes fieldAttributes, double formWidth, double formHeight)
        {
            base.InitializeFromMetadata(fieldAttributes, formWidth, formHeight);

            Name = fieldAttributes.Name;
            FieldWrapper = "div";
            Wrap = true;
            Html = fieldAttributes.PromptText;
            CssClass = "EpiLabel";
            fontSize = fieldAttributes.ControlFontSize;
            fontfamily = fieldAttributes.ControlFontFamily;
            fontstyle = fieldAttributes.ControlFontStyle;
            Height = formHeight * fieldAttributes.ControlHeightPercentage;
            Width = formWidth * fieldAttributes.ControlWidthPercentage;
            IsHidden = fieldAttributes.IsHidden;
        }

        public override string RenderHtml()
        {
            if (Wrap)
            {
                var wrapper = new TagBuilder(_fieldWrapper);
                if (string.IsNullOrEmpty(this._cssClass))
                {
                    wrapper.Attributes["class"] = _fieldWrapperClass;
                }
                else
                {
                    wrapper.Attributes["class"] = this._cssClass;
                }

                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"(\r\n|\r|\n)+");

                string newText = regex.Replace(Html.Replace("  ", " &nbsp;"), "<br />");

                Html = MvcHtmlString.Create(newText).ToString();

                // wrapper.Attributes["ID"] = "labelmvcdynamicfield_" + Name.ToLower();
                wrapper.Attributes["ID"] = "mvcdynamicfield_" + Name.ToLower() + "_fieldWrapper";
                StringBuilder StyleValues = new StringBuilder();

                StyleValues.Append(GetContolStyle(_fontstyle.ToString(), _top.ToString(), _left.ToString(), Width.ToString(), Height.ToString(), IsHidden));
                //StyleValues.Append(";word-wrap:break-word;");
                wrapper.Attributes.Add(new KeyValuePair<string, string>("style", StyleValues.ToString()));

                wrapper.InnerHtml = Html;
                return wrapper.ToString();
            }
            return Html;
        }
    }
}
