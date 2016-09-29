using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Epi.Cloud.Common.Metadata;
using Epi.Core.EnterInterpreter;
using Epi.DataPersistence.DataStructures;
using Epi.FormMetadata.Constants;
using Epi.FormMetadata.DataStructures;
using Epi.Web.Enter.Common.DTO;
using MvcDynamicForms;
using MvcDynamicForms.Fields;

namespace Epi.Web.MVC.Utility
{
    public class FormProvider
    {
        [ThreadStatic]
        public static List<SurveyAnswerDTO> SurveyAnswerList = null;

        [ThreadStatic]
        public static List<SurveyInfoDTO> SurveyInfoList = new List<SurveyInfoDTO>();

        public virtual Form GetForm(SurveyInfoDTO surveyInfo, int pageNumber, Epi.Web.Enter.Common.DTO.SurveyAnswerDTO surveyAnswer, bool isAndroid = false)
        {
            return GetForm(surveyInfo, pageNumber, surveyAnswer, SurveyAnswerList, SurveyInfoList, isAndroid);
        }
        public virtual Form GetForm(SurveyInfoDTO surveyInfo, int pageNumber, Epi.Web.Enter.Common.DTO.SurveyAnswerDTO surveyAnswer, List<SurveyAnswerDTO> surveyAnswerList, List<SurveyInfoDTO> surveyInfoList, bool isAndroid)
        {
            // Save last values for subsequent calls from ValidateAll in SurveyController
            SurveyAnswerList = surveyAnswerList;
            SurveyInfoList = surveyInfoList;

            FormDigest currentFormDigest = surveyInfo.GetCurrentFormDigest();
            PageDigest currentPageDigest = surveyInfo.GetCurrentFormPageDigestByPageNumber(pageNumber);

            var pageId = currentPageDigest.PageId;

            string formId = currentFormDigest.FormId;
            string formName = currentFormDigest.FormName;
            FormResponseDetail formResponseDetail = surveyAnswer.ResponseDetail != null
                ? surveyAnswer.ResponseDetail
                : new FormResponseDetail { FormId = formId, FormName = formName };

            PageResponseDetail pageResponseDetail = formResponseDetail.GetPageResponseDetailByPageId(pageId);

            var form = new Form();
            form.PageId = pageId.ToString();
            form.IsAndroid = isAndroid;
            form.ResponseId = surveyAnswer.ResponseId;

            form.SurveyInfo = surveyInfo;

            //Watermark 
            if (form.SurveyInfo.IsDraftMode)
            {
                form.IsDraftModeStyleClass = "draft";
            }

            form.CurrentPage = pageNumber;

            form.NumberOfPages = currentFormDigest.NumberOfPages;

            double _Width, _Height;

            var orientationIsPortrait = currentFormDigest.Orientation == "Portrait";
            _Height = orientationIsPortrait ? currentFormDigest.Height : currentFormDigest.Width;
            _Width = orientationIsPortrait ? currentFormDigest.Width : currentFormDigest.Height;

            //Add checkcode to Form
            string checkcode = currentFormDigest.CheckCode;

            StringBuilder javaScript = new StringBuilder();
            StringBuilder VariableDefinitions = new StringBuilder();
            string defineFormat = "cce_Context.define(\"{0}\", \"{1}\", \"{2}\", \"{3}\");";
            string defineNumberFormat = "cce_Context.define(\"{0}\", \"{1}\", \"{2}\", new Number({3}));";

            if (surveyAnswerList != null && surveyAnswerList.Count > 0)
            {
                form.FormCheckCodeObj = form.GetRelateCheckCodeObj(GetRelateFormObj(surveyAnswerList, surveyInfoList), checkcode);
            }
            else
            {
                form.FormCheckCodeObj = form.GetCheckCodeObj(surveyInfo.CurrentFormFieldDigests, formResponseDetail, checkcode);
            }

            form.HiddenFieldsList = formResponseDetail.HiddenFieldsList;
            form.HighlightedFieldsList = formResponseDetail.HighlightedFieldsList;
            form.DisabledFieldsList = formResponseDetail.DisabledFieldsList;
            form.RequiredFieldsList = formResponseDetail.RequiredFieldsList;

            form.FormCheckCodeObj.GetVariableJavaScript(VariableDefinitions);
            form.FormCheckCodeObj.GetSubroutineJavaScript(VariableDefinitions);
            string pageName = currentPageDigest.PageName;

            //Generate page level Java script (Before)
            javaScript.Append(GetPageLevelJS(pageNumber, form, pageName, "Before"));
            //Generate page level Java script (After)
            javaScript.Append(GetPageLevelJS(pageNumber, form, pageName, "After"));

            SetProviderSpecificProperties(form, _Height, _Width);

            var responseQA = pageResponseDetail != null ? pageResponseDetail.ResponseQA : new Dictionary<string, string>(); 
            AddFormFields(surveyInfo, pageId, responseQA, form, _Width, _Height, checkcode, javaScript);

            form.FormJavaScript = VariableDefinitions.ToString() + "\n" + javaScript.ToString();

            return form;
        }

        protected virtual void SetProviderSpecificProperties(Form form, double height, double width)
        {
            form.Height = height;
            form.Width = width;
        }

        protected virtual void AddFormFields(SurveyInfoDTO surveyInfo, int pageId, Dictionary<string, string> responseQA, Form form, double _Width, double _Height, string checkcode, StringBuilder javaScript)
        {
            IEnumerable<FieldAttributes> currentPageFieldAttributes = surveyInfo.GetCurrentFormPageFieldAttributesByPageId(pageId).ToArray();

            foreach (var fieldAttributes in currentPageFieldAttributes)
            {
                string fieldValue = null;

                fieldValue = (responseQA.TryGetValue(fieldAttributes.FieldName.ToLower(), out fieldValue) ? fieldValue : string.Empty);

                javaScript.Append(GetFormJavaScript(checkcode, form, fieldAttributes.FieldName));

                switch (fieldAttributes.FieldType)
                {
                    case FieldTypes.Text:   // textbox
                        form.AddFields(GetTextBox(fieldAttributes, _Width, _Height, fieldValue));
                        break;

                    case FieldTypes.Label:  //Label/Title
                        form.AddFields(GetLabel(fieldAttributes, _Width, _Height));
                        break;

                    case FieldTypes.UppercaseText:
                        break;

                    case FieldTypes.Multiline:  //MultiLineTextBox
                        form.AddFields(GetTextArea(fieldAttributes, _Width, _Height, fieldValue));
                        break;

                    case FieldTypes.Number: //NumericTextBox

                        form.AddFields(GetNumericTextBox(fieldAttributes, _Width, _Height, fieldValue));
                        break;

                    case FieldTypes.Date:   //DatePicker
                        form.AddFields(GetDatePicker(fieldAttributes, _Width, _Height, fieldValue));
                        break;

                    case FieldTypes.Time: //TimePicker
                        form.AddFields(GetTimePicker(fieldAttributes, _Width, _Height, fieldValue));
                        break;

                    case FieldTypes.DateTime:
                        break;

                    case FieldTypes.Checkbox: //CheckBox
                        var checkbox = GetCheckBox(fieldAttributes, _Width, _Height, fieldValue);
                        form.AddFields(checkbox);
                        break;

                    case FieldTypes.YesNo:  //DropDown Yes/No
                        if (fieldValue == "1" || fieldValue == "true")
                        {
                            fieldValue = "Yes";
                        }
                        else if (fieldValue == "0" || fieldValue == "false")
                        {
                            fieldValue = "No";
                        }

                        var dropdownSelectedValueYN = GetDropDown(fieldAttributes, _Width, _Height, fieldValue, "Yes&#;No", 11);
                        form.AddFields(dropdownSelectedValueYN);
                        break;

                    case FieldTypes.Option: //RadioList
                        AddRadioButtonGroupBox(form, fieldAttributes, _Width, _Height);
                        var selectedRadioListValue = fieldValue;
                        var radioListValues = fieldAttributes.List;
                        form.AddFields(GetRadioList(fieldAttributes, _Width, _Height, selectedRadioListValue));

                        break;

                    case FieldTypes.LegalValues:    //DropDown LegalValues
                        var dropDownLegalValues = string.Join("&#;", fieldAttributes.SourceTableValues).ToLower();
                        var selectedLegalValue = fieldValue;
                        form.AddFields(GetDropDown(fieldAttributes, _Width, _Height, selectedLegalValue, dropDownLegalValues, (int)FieldTypes.LegalValues));
                        break;

                    case FieldTypes.Codes:  //DropDown Codes
                        var dropDownCodesValues = string.Join("&#;", fieldAttributes.SourceTableValues).ToLower();
                        var selectedCodesValue = fieldValue;
                        var dropDownSelectedCodesValue = GetDropDown(fieldAttributes, _Width, _Height, selectedCodesValue, dropDownCodesValues, (int)FieldTypes.Codes);
                        form.AddFields(dropDownSelectedCodesValue);
                        break;

                    case FieldTypes.CommentLegal:   //DropDown CommentLegal
                        var dropDownCommentLegalValues = string.Join("&#;", fieldAttributes.SourceTableValues).ToLower();
                        var selectedCommentLegalValue = fieldValue;
                        var dropDownSelectedCommentLegalValue = GetDropDown(fieldAttributes, _Width, _Height, selectedCommentLegalValue, dropDownCommentLegalValues, 19);
                        form.AddFields(dropDownSelectedCommentLegalValue);
                        break;

                    case FieldTypes.Relate: //RelateButton
                        form.AddFields(GetRelateButton(fieldAttributes, _Width, _Height));
                        break;

                    case FieldTypes.Group:  //GroupBox
                        form.AddFields(GetGroupBox(fieldAttributes, _Width, _Height, fieldValue));
                        break;
                }
            }
        }

        protected virtual void AddRadioButtonGroupBox(Form form, FieldAttributes fieldAttributes, double _Width, double _Height)
        {
            var radioGroupBoxValue = string.Empty;
            form.AddFields(GetGroupBox(fieldAttributes, _Width + 12, _Height, radioGroupBoxValue));
        }

        public virtual void UpdateHiddenFields(int pageNumber, Form form, MetadataAccessor metadataAccessor, System.Collections.Specialized.NameValueCollection postedForm)
        {
            double _Width, _Height;
            _Width = 1024;
            _Height = 768;

            var currentFieldDigests = metadataAccessor.GetCurrentFormFieldDigestsWithPageNumber(pageNumber);
            var otherPageFieldDigests = metadataAccessor.GetCurrentFormFieldDigestsNotWithPageNumber(pageNumber);
            foreach (var fieldDigest in otherPageFieldDigests)
            {
                bool IsFound = false;
                string Value = null;

                foreach (var key in postedForm.AllKeys.Where(x => x.StartsWith(form.FieldPrefix)))
                {
                    string fieldKey = key.Remove(0, form.FieldPrefix.Length);

                    if (fieldKey.Equals(fieldDigest.FieldName, StringComparison.OrdinalIgnoreCase))
                    {
                        Value = postedForm[key];
                        IsFound = true;
                        break;
                    }
                }

                if (IsFound)
                {
                    var fieldAttributes = metadataAccessor.GetFieldAttributes(fieldDigest);

                    MvcDynamicForms.Fields.Field field = null;
                    

                    switch ((int)fieldDigest.FieldType)
                    {
                        case 1: // textbox
                            var _TextBoxValue = Value;
                            field = GetTextBox(fieldAttributes, _Width, _Height, _TextBoxValue);
                            break;

                        case 2: //Label/Title
                            field = GetLabel(fieldAttributes, _Width, _Height);
                            break;

                        case 3: //Label
                            break;

                        case 4: //MultiLineTextBox
                            var _TextAreaValue = Value;
                            field = GetTextArea(fieldAttributes, _Width, _Height, _TextAreaValue);
                            break;

                        case 5: //NumericTextBox
                            var _NumericTextBoxValue = Value;
                            field = GetNumericTextBox(fieldAttributes, _Width, _Height, _NumericTextBoxValue);
                            break;

                        case 7: // 7 DatePicker
                            var _DatePickerValue = Value;
                            field = GetDatePicker(fieldAttributes, _Width, _Height, _DatePickerValue);
                            break;

                        case 8: //TimePicker
                            var _timePickerValue = Value;
                            field = GetTimePicker(fieldAttributes, _Width, _Height, _timePickerValue);
                            break;

                        case 10://CheckBox
                            var _CheckBoxValue = Value;
                            field = GetCheckBox(fieldAttributes, _Width, _Height, _CheckBoxValue);
                            break;

                        case 11://DropDown Yes/No
                            var _DropDownSelectedValueYN = Value;
                            if (_DropDownSelectedValueYN == "1")
                            {
                                _DropDownSelectedValueYN = "Yes";
                            }

                            if (_DropDownSelectedValueYN == "0")
                            {

                                _DropDownSelectedValueYN = "No";
                            }
                            var dropdownselectedvalueYN = GetDropDown(fieldAttributes, _Width, _Height, _DropDownSelectedValueYN, "Yes&#;No", 11);

                            form.AddFields(dropdownselectedvalueYN);
                            break;

                        case 12: //RadioList
                            var _RadioListSelectedValue1 = Value;
                            string RadioListValues1 = "";
                            field = GetRadioList(fieldAttributes, _Width, _Height, _RadioListSelectedValue1);
                            break;

                        case 17: //DropDown LegalValues
                            string DropDownValues1 = "";
                            var _DropDownSelectedValue1 = Value;
                            field = GetDropDown(fieldAttributes, _Width, _Height, _DropDownSelectedValue1, DropDownValues1, 17);
                            break;

                        case 18: //DropDown Codes
                            string DropDownValues2 = "";
                            var _DropDownSelectedValue2 = Value;
                            field = GetDropDown(fieldAttributes, _Width, _Height, _DropDownSelectedValue2, DropDownValues2, 18);
                            break;

                        case 19: //DropDown CommentLegal
                            string DropDownValues = "";
                            var _DropDownSelectedValue = Value;
                            field = GetDropDown(fieldAttributes, _Width, _Height, _DropDownSelectedValue, DropDownValues, 19);
                            break;

                        case 21: //GroupBox
                            field = GetGroupBox(fieldAttributes, _Width, _Height, Value);
                            break;
                    }

                    if (field != null)
                    {
                        field.IsPlaceHolder = true;
                        form.AddFields(field);
                    }
                }
            }
        }

 
        protected virtual MvcDynamicForms.Fields.Field GetRadioList(FieldAttributes fieldAttributes, double formWidth, double formHeight, string controlValue)
        {
            var radiolist = new RadioList(fieldAttributes, formWidth, formHeight)
            {
                Value = controlValue
            };

            return radiolist;
        }

        protected virtual MvcDynamicForms.Fields.Field GetNumericTextBox(FieldAttributes fieldAttributes, double formWidth, double formHeight, string controlValue)
        {
            var numericTextBox = new NumericTextBox(fieldAttributes, formWidth, formHeight)
            {
                Value = controlValue
            };

            return numericTextBox;
        }

        protected virtual MvcDynamicForms.Fields.Field GetLabel(FieldAttributes fieldAttributes, double formWidth, double formHeight)
        {
            var label = new Literal(fieldAttributes, formWidth, formHeight);
            return label;
        }

        protected virtual MvcDynamicForms.Fields.Field GetTextArea(FieldAttributes fieldAttributes, double formWidth, double formHeight, string controlValue)
        {
            var textArea = new TextArea(fieldAttributes, formWidth, formHeight)
            {
                Value = controlValue
            };
            return textArea;
        }

        protected virtual MvcDynamicForms.Fields.Field GetTextBox(FieldAttributes fieldAttributes, double formWidth, double formHeight, string controlValue)
        {
            var textBox = new TextBox(fieldAttributes, formWidth, formHeight)
            {
                Value = controlValue
            };

            return textBox;
        }

        protected virtual MvcDynamicForms.Fields.Field GetCheckBox(FieldAttributes fieldAttributes, double formWidth, double formHeight, string controlValue)
        {
            var checkBox = new CheckBox(fieldAttributes, formWidth, formHeight)// CheckBox (fieldAttributes, formWidth, formHeight)
            {
                Value = controlValue,
                Response = controlValue
            };

            return checkBox;
        }

        protected virtual MvcDynamicForms.Fields.Field GetDatePicker(FieldAttributes fieldAttributes, double formWidth, double formHeight, string controlValue)
        {
            var DatePicker = new DatePicker(fieldAttributes, formWidth, formHeight)
            {
                Value = controlValue,
                Response = controlValue
            };

            return DatePicker;
        }

        protected virtual MvcDynamicForms.Fields.Field GetTimePicker(FieldAttributes fieldAttributes, double formWidth, double formHeight, string controlValue)
        {
            var TimePicker = new TimePicker(fieldAttributes, formWidth, formHeight)
            {
                Value = controlValue,
                Response = controlValue
            };

            return TimePicker;
        }

         protected virtual MvcDynamicForms.Fields.Field GetRelateButton(FieldAttributes fieldAttributes, double formWidth, double formHeight)
        {
            var RelateButton = new RelateButton(fieldAttributes, formWidth, formHeight)
            {
                // Value = controlValue
            };

            return RelateButton;
        }

        protected virtual MvcDynamicForms.Fields.Field GetDropDown(FieldAttributes fieldAttributes, double formWidth, double formHeight, string controlValue, string DropDownValues, int FieldTypeId)
        {
            var select = new Select(fieldAttributes, formWidth, formHeight)//, DropDownValues, FieldTypeId)
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

        protected virtual MvcDynamicForms.Fields.Field GetGroupBox(FieldAttributes fieldAttributes, double formWidth, double formHeight, string controlValue)
        {
            var groupbox = new GroupBox(fieldAttributes, formWidth, formHeight)
            {
                Value = controlValue
            };

            return groupbox;
        }

        protected virtual string GetFormJavaScript(string CheckCode, Form form, string controlName)
        {// controlName
            StringBuilder B_JavaScript = new StringBuilder();
            EnterRule FunctionObject_B = (EnterRule)form.FormCheckCodeObj.GetCommand("level=field&event=before&identifier=" + controlName);
            if (FunctionObject_B != null && !FunctionObject_B.IsNull())
            {
                B_JavaScript.Append("function " + controlName.ToLower());
                FunctionObject_B.ToJavaScript(B_JavaScript);
            }

            StringBuilder A_JavaScript = new StringBuilder();
            EnterRule FunctionObject_A = (EnterRule)form.FormCheckCodeObj.GetCommand("level=field&event=after&identifier=" + controlName);
            if (FunctionObject_A != null && !FunctionObject_A.IsNull())
            {
                A_JavaScript.Append("function " + controlName.ToLower());
                FunctionObject_A.ToJavaScript(A_JavaScript);
            }

            EnterRule FunctionObject = (EnterRule)form.FormCheckCodeObj.GetCommand("level=field&event=click&identifier=" + controlName);
            if (FunctionObject != null && !FunctionObject.IsNull())
            {
                A_JavaScript.Append("function " + controlName.ToLower());
                FunctionObject.ToJavaScript(A_JavaScript);
            }

            return B_JavaScript.ToString() + "  " + A_JavaScript.ToString();
        }

        protected static string GetPageLevelJS(int pageNumber, Form form, string pageName, string beforeOrAfter)
        {
            StringBuilder JavaScript = new StringBuilder();
            if (beforeOrAfter == "Before")
            {
                Epi.Core.EnterInterpreter.Rules.Rule_Begin_Before_Statement FunctionObject_B = (Epi.Core.EnterInterpreter.Rules.Rule_Begin_Before_Statement)form.FormCheckCodeObj.GetCommand("level=page&event=before&identifier=" + pageName);
                if (FunctionObject_B != null && !FunctionObject_B.IsNull())
                {

                    JavaScript.Append("$(document).ready(function () {  ");
                    JavaScript.Append("page" + pageNumber + "_before();");
                    JavaScript.Append("});");

                    JavaScript.Append("\n\nfunction page" + pageNumber);
                    FunctionObject_B.ToJavaScript(JavaScript);


                }
            }
            if (beforeOrAfter == "After")
            {
                Epi.Core.EnterInterpreter.Rules.Rule_Begin_After_Statement FunctionObject_A = (Epi.Core.EnterInterpreter.Rules.Rule_Begin_After_Statement)form.FormCheckCodeObj.GetCommand("level=page&event=after&identifier=" + pageName);
                if (FunctionObject_A != null && !FunctionObject_A.IsNull())
                {
                    JavaScript.AppendLine("$(document).ready(function () {");
                    //JavaScript.AppendLine("$('#myform').submit(function () {");
                    //  JavaScript.AppendLine("page" + PageNumber + "_after();})");
                    JavaScript.AppendLine("$(\"[href]\").click(function(e) {");
                    JavaScript.AppendLine("page" + pageNumber + "_after();  e.preventDefault(); })");

                    JavaScript.AppendLine("$(\"#ContinueButton\").click(function(e) {");
                    JavaScript.AppendLine("page" + pageNumber + "_after();  e.preventDefault(); })");

                    JavaScript.AppendLine("$(\"#PreviousButton\").click(function(e) {");
                    JavaScript.AppendLine("page" + pageNumber + "_after();  e.preventDefault(); })");
                    JavaScript.AppendLine("});");

                    JavaScript.Append("\n\nfunction page" + pageNumber);
                    FunctionObject_A.ToJavaScript(JavaScript);

                }
            }

            return JavaScript.ToString();
        }


        protected static Dictionary<string, bool> GetChoices(List<string> List)
        {
            Dictionary<string, bool> NewList = new Dictionary<string, bool>();
            foreach (var _List in List)
            {
                NewList.Add(_List, false);
            }
            return NewList;
        }

        protected static List<Epi.Web.Enter.Common.Helper.RelatedFormsObj> GetRelateFormObj(List<SurveyAnswerDTO> surveyAnswerList, List<SurveyInfoDTO> surveyInfoList)
        {
            List<Epi.Web.Enter.Common.Helper.RelatedFormsObj> List = new List<Enter.Common.Helper.RelatedFormsObj>();

            for (int i = 0; surveyAnswerList.Count() > i; i++)
            {
                Epi.Web.Enter.Common.Helper.RelatedFormsObj RelatedFormsObj = new Epi.Web.Enter.Common.Helper.RelatedFormsObj();

                MetadataAccessor metadataAccessor = surveyInfoList[i] as MetadataAccessor;
                RelatedFormsObj.FieldDigests = metadataAccessor.CurrentFormFieldDigests;
                RelatedFormsObj.ResponseDetail = surveyAnswerList[i].ResponseDetail ?? new FormResponseDetail();

                List.Add(RelatedFormsObj);
            }

            return List;
        }
    }
}
