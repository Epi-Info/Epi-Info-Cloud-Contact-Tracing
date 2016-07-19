using System;
using Epi.Web.MVC.Repositories.Core;
using Epi.Web.Enter.Common.Message;
using Epi.Web.MVC.Constants;
using Epi.Web.MVC.Models;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;
using System.Xml.XPath;
using System.Text.RegularExpressions;
using System.Web.Security;
using Epi.Cloud.DataEntryServices.Model;
using Epi.Cloud.Common.EntityObjects;
using Epi.Cloud.Common.Constants;

namespace Epi.Web.MVC.Utility
{
    public class SurveyHelper
    {
        /// <summary>
        /// Creates the first survey response in the response table
        /// </summary>
        /// <param name="surveyId"></param>
        /// <param name="responseId"></param>
        /// <param name="surveyAnswerRequest"></param>
        /// <param name="surveyAnswerDTO"></param>
        /// <param name="surveyResponseHelper"></param>
        /// <param name="iSurveyAnswerRepository"></param>
        public static Epi.Web.Enter.Common.DTO.SurveyAnswerDTO CreateSurveyResponse(string surveyId, string responseId, SurveyAnswerRequest surveyAnswerRequest1,
                                          Enter.Common.DTO.SurveyAnswerDTO surveyAnswerDTO,
                                          SurveyResponseDocDb surveyResponseHelper, ISurveyAnswerRepository iSurveyAnswerRepository, int UserId, bool IsChild = false, string RelateResponseId = "", bool IsEditMode = false, int CurrentOrgId = -1)
        {
            bool AddRoot = false;
            SurveyAnswerRequest surveyAnswerRequest = new SurveyAnswerRequest();
            surveyAnswerRequest.Criteria.SurveyAnswerIdList.Add(responseId.ToString());
            surveyAnswerDTO.ResponseId = responseId.ToString();
            surveyAnswerDTO.DateCreated = DateTime.UtcNow;
            surveyAnswerDTO.SurveyId = surveyId;
            surveyAnswerDTO.Status = RecordStatus.InProcess;
            surveyAnswerDTO.RecordSourceId = 1;
            if (IsEditMode)
            {
                surveyAnswerDTO.ParentRecordId = RelateResponseId;
            }
            //if (IsEditMode)
            //    {
            //    surveyAnswerDTO.Status = RecordStatus.Complete;
            //    }
            //else
            //    {
            //    surveyAnswerDTO.Status = RecordStatus.InProcess;
            //    }

            XmlDocument xml;
            FormResponseDetail responseDetail = surveyResponseHelper.CreateResponseDetail(surveyId, AddRoot, 0, "", out xml);
            surveyAnswerDTO.ResponseDetail = responseDetail;

            surveyAnswerDTO.XML = xml.InnerXml;
            surveyAnswerDTO.RelateParentId = RelateResponseId;
            surveyAnswerRequest.Criteria.UserId = UserId;
            surveyAnswerRequest.Criteria.UserOrganizationId = CurrentOrgId;
            surveyAnswerRequest.SurveyAnswerList.Add(surveyAnswerDTO);
            if (!IsChild)
            {
                surveyAnswerRequest.Action = Epi.Web.MVC.Constants.Constant.CREATE;
            }
            else
            {
                if (IsEditMode)
                {

                    surveyAnswerRequest.SurveyAnswerList[0].ParentRecordId = null;
                }

                surveyAnswerRequest.Action = Epi.Web.MVC.Constants.Constant.CREATECHILD;

            }

            iSurveyAnswerRepository.SaveSurveyAnswer(surveyAnswerRequest);

            return surveyAnswerDTO;
        }

        public static void UpdateSurveyResponse(SurveyInfoModel surveyInfoModel, MvcDynamicForms.Form form,
                                                SurveyAnswerRequest surveyAnswerRequest,
                                                SurveyResponseDocDb surveyResponseHelper,
                                                ISurveyAnswerRepository iSurveyAnswerRepository,
                                                SurveyAnswerResponse surveyAnswerResponse,
                                                string responseId,
                                                Epi.Web.Enter.Common.DTO.SurveyAnswerDTO surveyAnswerDTO,
                                                bool IsSubmited,
                                                bool IsSaved,
                                                int PageNumber,
                                                int UserId)
        {
            // 1 Get the record for the current survey response
            // 2 update the current survey response
            // 3 save the current survey response

            var savedResponseDetail = surveyAnswerDTO.ResponseDetail;

            if (!IsSubmited)
            {

                // 2 a. update the current survey answer request
                surveyAnswerRequest.SurveyAnswerList = surveyAnswerResponse.SurveyResponseList;

                surveyResponseHelper.Add(form);
                bool addRoot = false;
                XDocument savedXml = null;
                if (surveyAnswerDTO.XML != null)
                {
                    savedXml = XDocument.Parse(surveyAnswerDTO.XML);
                    if (savedXml.Root.FirstAttribute.Value.ToString() == "0")
                    {
                        addRoot = true;
                    }
                }

                XmlDocument xml;
                FormResponseDetail responseDetail = surveyResponseHelper.CreateResponseDetail(surveyInfoModel.SurveyId, addRoot, form.CurrentPage, form.PageId, out xml);

                surveyAnswerRequest.SurveyAnswerList[0].ResponseDetail = responseDetail;
                surveyAnswerRequest.SurveyAnswerList[0].XML = xml != null ? xml.InnerXml : null;
                // 2 b. save the current survey response
                surveyAnswerRequest.Action = Epi.Web.MVC.Constants.Constant.UPDATE;  //"Update";
                                                                                     // surveyAnswerRequest.Action = Epi.Web.MVC.Constants.Constant.UpdateMulti; 
                                                                                     //Append to Response Xml
                var currentPageNumber = form.CurrentPage;
                FormResponseDetail currentFormResponseDetail = surveyAnswerRequest.SurveyAnswerList[0].ResponseDetail;
                PageResponseDetail currentPageResponseDetail = currentFormResponseDetail.GetPageResponseDetailByPageNumber(currentPageNumber);
                XDocument currentPageResponseXml = surveyAnswerRequest.SurveyAnswerList[0].XML != null ? XDocument.Parse(surveyAnswerRequest.SurveyAnswerList[0].XML) : null;
                if (addRoot == false)
                {
                    //surveyAnswerRequest.SurveyAnswerList[0].XML = MergeXml(savedXml, currentPageResponseXml, currentPageNumber).ToString();
                    surveyAnswerRequest.SurveyAnswerList[0].ResponseDetail = MergeResponseDetail(savedResponseDetail, currentPageResponseDetail, currentPageNumber);
                }
            }

            var updatedFromResponseDetail = surveyAnswerRequest.SurveyAnswerList[0].ResponseDetail;

            ////Update page number before saving response 
            if (surveyAnswerRequest.SurveyAnswerList[0].CurrentPageNumber != 0)
            {
                updatedFromResponseDetail.LastPageVisited = PageNumber;
            }
            if (form.HiddenFieldsList != null)
            {
                updatedFromResponseDetail.HiddenFieldsList = form.HiddenFieldsList;
            }
            if (form.HighlightedFieldsList != null)
            {
                updatedFromResponseDetail.HighlightedFieldsList = form.HighlightedFieldsList;
            }
            if (form.DisabledFieldsList != null)
            {
                updatedFromResponseDetail.DisabledFieldsList = form.DisabledFieldsList;
            }
            if (form.RequiredFieldsList != null)
            {
                updatedFromResponseDetail.RequiredFieldsList = form.RequiredFieldsList;
            }

            //#region Xml
            //if (surveyAnswerRequest.SurveyAnswerList[0].XML != null)
            //{
            //    XDocument Xdoc = XDocument.Parse(surveyAnswerRequest.SurveyAnswerList[0].XML);
            //    if (PageNumber != 0)
            //    {
            //        Xdoc.Root.Attribute("LastPageVisited").Value = PageNumber.ToString();
            //    }

            //    ////Update Hidden Fields List before saving response XML
            //    if (form.HiddenFieldsList != null)
            //    {
            //        Xdoc.Root.Attribute("HiddenFieldsList").Value = "";
            //        Xdoc.Root.Attribute("HiddenFieldsList").Value = form.HiddenFieldsList.ToString();
            //    }
            //    if (form.HighlightedFieldsList != null)
            //    {
            //        Xdoc.Root.Attribute("HighlightedFieldsList").Value = "";
            //        Xdoc.Root.Attribute("HighlightedFieldsList").Value = form.HighlightedFieldsList.ToString();
            //    }
            //    if (form.DisabledFieldsList != null)
            //    {
            //        Xdoc.Root.Attribute("DisabledFieldsList").Value = "";
            //        Xdoc.Root.Attribute("DisabledFieldsList").Value = form.DisabledFieldsList.ToString();
            //    }
            //    if (form.RequiredFieldsList != null)
            //    {
            //        Xdoc.Root.Attribute("RequiredFieldsList").Value = "";
            //        Xdoc.Root.Attribute("RequiredFieldsList").Value = form.RequiredFieldsList.ToString();
            //    }


            //    //  AssignList 

            //    // TODO Must be implemented without XML
            //    List<KeyValuePair<string, String>> FieldsList = new List<KeyValuePair<string, string>>();

            //    FieldsList = GetHiddenFieldsList(form);
            //    if (FieldsList != null)
            //    {
            //        IEnumerable<XElement> XElementList = Xdoc.XPathSelectElements("SurveyResponse/Page/ResponseDetail");
            //        for (var i = 0; i < FieldsList.Count; i++)
            //        {
            //            foreach (XElement Element in XElementList)
            //            {
            //                if (Element.Attribute("QuestionName").Value.ToString().Equals(FieldsList[i].Key, StringComparison.OrdinalIgnoreCase))
            //                {
            //                    if (FieldsList[i].Value != null)
            //                    {
            //                        Element.Value = FieldsList[i].Value;
            //                    }
            //                    break;
            //                }
            //            }
            //        }
            //    }

            //    ////Update survey response Status
            //    //if (IsSubmited)
            //    //{

            //    //    surveyAnswerRequest.SurveyAnswerList[0].Status = RecordStatus.Submitted;
            //    //    surveyAnswerRequest.SurveyAnswerList[0].DateCompleted = DateTime.Now;
            //    //    Xdoc.Root.Attribute("LastPageVisited").Remove();
            //    //    Xdoc.Root.Attribute("HiddenFieldsList").Remove();
            //    //    Xdoc.Root.Attribute("HighlightedFieldsList").Remove();
            //    //    Xdoc.Root.Attribute("DisabledFieldsList").Remove();
            //    //    Xdoc.Root.Attribute("RequiredFieldsList").Remove(); 
            //    //    RemovePageNumAtt(Xdoc);
            //    //}
            //    if (IsSaved)
            //    {
            //        surveyAnswerRequest.SurveyAnswerList[0].Status = RecordStatus.Saved;
            //    }
            //    surveyAnswerRequest.SurveyAnswerList[0].XML = Xdoc.ToString();
            //    /////Update Survey Mode ////////////////////
            //    surveyAnswerRequest.SurveyAnswerList[0].IsDraftMode = surveyAnswerDTO.IsDraftMode;
            //    surveyAnswerRequest.Criteria.UserId = UserId;
            //    iSurveyAnswerRepository.SaveSurveyAnswer(surveyAnswerRequest);
            //}
            //#endregion Xml

        }

        //Remove PageNumber attribute
        private static void RemovePageNumAtt(XDocument Xdoc)
        {
            var _Pages = from _Page in Xdoc.Descendants("Page") select _Page;

            foreach (var _Page in _Pages)
            {

                _Page.Attribute("PageNumber").Remove();
            }



        }





        /// <summary>
        /// Returns a SurveyInfoDTO object
        /// </summary>
        /// <param name="surveyInfoRequest"></param>
        /// <param name="iSurveyInfoRepository"></param>
        /// <param name="SurveyId"></param>
        /// <returns></returns>
        public static Epi.Web.Enter.Common.DTO.SurveyInfoDTO GetSurveyInfoDTO(SurveyInfoRequest surveyInfoRequest,
                                                  ISurveyInfoRepository iSurveyInfoRepository,
                                                  string SurveyId)
        {
            surveyInfoRequest.Criteria.SurveyIdList.Add(SurveyId);
            return iSurveyInfoRepository.GetSurveyInfo(surveyInfoRequest).SurveyInfoList[0];
        }

        //public static XDocument MergeXml(XDocument SavedXml, XDocument CurrentPageResponseXml, int Pagenumber)
        //{

        //    XDocument xdoc = XDocument.Parse(SavedXml.ToString());
        //    XElement oldXElement = xdoc.XPathSelectElement("SurveyResponse/Page[@PageNumber = '" + Pagenumber.ToString() + "']");


        //    if (oldXElement == null)
        //    {
        //        SavedXml.Root.Add(CurrentPageResponseXml.Elements());
        //        return SavedXml;
        //    }

        //    else 
        //    {
        //        oldXElement.Remove();
        //        xdoc.Root.Add(CurrentPageResponseXml.Elements());
        //        return xdoc;
        //    }


        //}


        /// <summary>
        ///   This function will loop through the form controls and checks if any of the controls are found in the context detail list. 
        ///   If any their values get updated from the context list.
        /// </summary>
        /// <param name="form"></param>
        /// <param name="ContextDetailList"></param>
        /// <returns>Returns a Form object</returns>
        public static MvcDynamicForms.Form UpdateControlsValuesFromContext(MvcDynamicForms.Form form, Dictionary<string, string> ContextDetailList)
        {



            Dictionary<string, string> formControlList = new Dictionary<string, string>();

            //var responses = new List<Response>();
            foreach (var field in form.InputFields)
            {
                string fieldName = field.Title;

                if (ContextDetailList.ContainsKey(fieldName))
                {
                    field.Response = ContextDetailList[fieldName].ToString();
                }

            }



            return form;
        }
        public static MvcDynamicForms.Form UpdateControlsValues(MvcDynamicForms.Form form, string Name, string Value)
        {

            foreach (var field in form.InputFields)
            {
                string fieldName = field.Title;

                if (Name.ToLower() == fieldName.ToLower())
                {
                    field.Response = Value.ToString();
                }

            }



            return form;
        }
        public static Dictionary<string, string> GetContextDetailList(Epi.Core.EnterInterpreter.EnterRule FunctionObject)
        {


            Dictionary<string, string> ContextDetailList = new Dictionary<string, string>();


            if (FunctionObject != null && !FunctionObject.IsNull())
            {

                foreach (KeyValuePair<string, EpiInfo.Plugin.IVariable> kvp in FunctionObject.Context.CurrentScope.SymbolList)
                {
                    EpiInfo.Plugin.IVariable field = kvp.Value;

                    if (!string.IsNullOrEmpty(field.Expression))
                    {
                        if (field.DataType == EpiInfo.Plugin.DataType.Date)
                        {

                            var datetemp = string.Format("{0:MM/dd/yyyy}", field.Expression);
                            DateTime date = new DateTime();
                            date = Convert.ToDateTime(datetemp);
                            ContextDetailList[kvp.Key] = date.Date.ToString("MM/dd/yyyy");
                        }
                        else
                        {
                            ContextDetailList[kvp.Key] = field.Expression;
                        }
                    }
                }
            }


            return ContextDetailList;
        }

        public static List<KeyValuePair<string, string>> GetHiddenFieldsList(MvcDynamicForms.Form pForm)
        {

            List<KeyValuePair<string, String>> FieldsList = new List<KeyValuePair<string, string>>();

            foreach (var field in pForm.InputFields)
            {
                if (field.IsPlaceHolder)
                {
                    FieldsList.Add(new KeyValuePair<string, string>(field.Title, field.Response));

                }
            }

            return FieldsList;
        }
        public static void UpdatePassCode(UserAuthenticationRequest AuthenticationRequest, ISurveyAnswerRepository iSurveyAnswerRepository)
        {


            iSurveyAnswerRepository.UpdatePassCode(AuthenticationRequest);

        }
        public static string GetPassCode()
        {

            Guid Guid = Guid.NewGuid();
            string Passcode = Guid.ToString().Substring(0, 4);
            return Passcode;
        }
        public static bool IsGuid(string expression)
        {
            if (expression != null)
            {
                Regex guidRegEx = new Regex(@"^(\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1})$");

                return guidRegEx.IsMatch(expression);
            }
            return false;
        }



        public static int GetNumberOfPages(string ResponseXml)
        {

            XDocument xdoc = XDocument.Parse(ResponseXml);
            int PageNumber = 0;
            PageNumber = xdoc.Root.Elements("Page").Count();

            return PageNumber;


        }

        public static bool IsMobileDevice(string RequestUserAgent)
        {


            if (RequestUserAgent.IndexOf("Opera Mobi", StringComparison.OrdinalIgnoreCase) >= 0 || RequestUserAgent.Contains("Opera Mobi"))
            {
                return true;
            }
            else if (RequestUserAgent.IndexOf("Android", StringComparison.OrdinalIgnoreCase) >= 0 || RequestUserAgent.Contains("Android"))
            {
                return true;
            }
            else if (RequestUserAgent.IndexOf("Mobile", StringComparison.OrdinalIgnoreCase) >= 0 || RequestUserAgent.Contains("Mobile"))
            {
                return true;
            }
            else if (RequestUserAgent.IndexOf("Phone", StringComparison.OrdinalIgnoreCase) >= 0 || RequestUserAgent.Contains("Phone"))
            {
                return true;
            }
            else if (RequestUserAgent.IndexOf("Opera Mini", StringComparison.OrdinalIgnoreCase) >= 0 || RequestUserAgent.Contains("Opera Mini"))
            {
                return true;
            }
            else
            {

                return false;
            }

        }

        public static string CreateResponseDocument(XDocument pMetaData, string pXML, string RequiredList)
        {
            XDocument XmlResponse = new XDocument();
            IEnumerable<XElement> PageFields = null;
            int NumberOfPages = GetNumberOfPages(pMetaData);
            for (int i = 0; NumberOfPages > i - 1; i++)
            {
                var _FieldsTypeIDs = from _FieldTypeID in
                                         pMetaData.Descendants("Field")
                                     where _FieldTypeID.Attribute("Position").Value == (i - 1).ToString()
                                     select _FieldTypeID;

                PageFields = _FieldsTypeIDs;

                XDocument CurrentPageXml = ToXDocument(CreateResponseXml("", false, i, "", PageFields, RequiredList));

                if (i == 0)
                {
                    XmlResponse = ToXDocument(CreateResponseXml("", true, i, "", PageFields, RequiredList));
                }
                else
                {
                    XmlResponse = MergeXml(XmlResponse, CurrentPageXml, i);
                }
            }

            return XmlResponse.ToString();
        }

        private static int GetNumberOfPages(XDocument Xml)
        {
            var _FieldsTypeIDs = from _FieldTypeID in
                                     Xml.Descendants("View")
                                 select _FieldTypeID;

            return _FieldsTypeIDs.Elements().Count();
        }

        public static XmlDocument CreateResponseXml(string SurveyId, bool AddRoot, int CurrentPage, string Pageid, IEnumerable<XElement> PageFields, string RequiredList)
        {
            XmlDocument xml = new XmlDocument();
            XmlElement root = xml.CreateElement("SurveyResponse");

            if (CurrentPage == 0)
            {
                root.SetAttribute("SurveyId", SurveyId);
                root.SetAttribute("LastPageVisited", "1");
                root.SetAttribute("HiddenFieldsList", "");
                root.SetAttribute("HighlightedFieldsList", "");
                root.SetAttribute("DisabledFieldsList", "");
                root.SetAttribute("RequiredFieldsList", "");

                xml.AppendChild(root);
            }

            XmlElement PageRoot = xml.CreateElement("Page");
            if (CurrentPage != 0)
            {
                PageRoot.SetAttribute("PageNumber", CurrentPage.ToString());
                PageRoot.SetAttribute("PageId", Pageid);//Added PageId Attribute to the page node
                PageRoot.SetAttribute("MetaDataPageId", Pageid.ToString());
                xml.AppendChild(PageRoot);
            }

            foreach (var Field in PageFields)
            {
                XmlElement child = xml.CreateElement(Epi.Web.MVC.Constants.Constant.RESPONSE_DETAILS);
                child.SetAttribute("QuestionName", Field.Attribute("Name").Value);
                child.InnerText = Field.Value;
                PageRoot.AppendChild(child);
                //Start Adding required controls to the list
                SetRequiredList(Field, RequiredList);
            }

            return xml;
        }

        public static XDocument ToXDocument(XmlDocument xmlDocument)
        {
            using (var nodeReader = new XmlNodeReader(xmlDocument))
            {
                nodeReader.MoveToContent();
                return XDocument.Load(nodeReader);
            }
        }

        public static XDocument MergeXml(XDocument SavedXml, XDocument CurrentPageResponseXml, int Pagenumber)
        {
            XDocument xdoc = XDocument.Parse(SavedXml.ToString());
            XElement oldXElement = xdoc.XPathSelectElement("SurveyResponse/Page[@PageNumber = '" + Pagenumber.ToString() + "']");

            if (oldXElement == null)
            {
                SavedXml.Root.Add(CurrentPageResponseXml.Elements());
                return SavedXml;
            }

            else
            {
                oldXElement.Remove();
                xdoc.Root.Add(CurrentPageResponseXml.Elements());
                return xdoc;
            }
        }

        public static FormResponseDetail MergeResponseDetail(FormResponseDetail savedResponseDetail, PageResponseDetail currentPageResponseDetail, int pageNumber)
        {
            savedResponseDetail = savedResponseDetail ?? new FormResponseDetail { FormId = currentPageResponseDetail.FormId, FormName = currentPageResponseDetail.FormName };
            var savedPageResponseDetail = savedResponseDetail.GetPageResponseDetailByPageNumber(pageNumber);
            if (savedPageResponseDetail != null)
            {
                savedResponseDetail.PageResponseDetailList.Remove(savedPageResponseDetail);
            }

            savedResponseDetail.AddPageResponseDetail(currentPageResponseDetail);

            return savedResponseDetail;
        }

        public static void SetRequiredList(XElement _Fields, string RequiredList)
        {
            bool isRequired = false;
            string value = _Fields.Attribute("IsRequired").Value;

            if (bool.TryParse(value, out isRequired))
            {
                if (isRequired)
                {
                    if (!RequiredList.Contains(_Fields.Attribute("Name").Value))
                    {
                        if (RequiredList != "")
                        {
                            RequiredList = RequiredList + "," + _Fields.Attribute("Name").Value.ToLower();
                        }
                        else
                        {
                            RequiredList = _Fields.Attribute("Name").Value.ToLower();
                        }
                    }
                }
            }
        }
        public static int GetDecryptUserId(string Id)
        {

            string DecryptedUserId = "";
            try
            {
                DecryptedUserId = Epi.Web.Enter.Common.Security.Cryptography.Decrypt(Id);
            }
            catch (Exception ex)
            {
                FormsAuthentication.SignOut();
                FormsAuthentication.RedirectToLoginPage();

            }
            int UserId = -1;
            int.TryParse(DecryptedUserId, out UserId);

            return UserId;
        }

    }
}