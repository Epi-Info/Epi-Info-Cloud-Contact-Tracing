using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Epi.Cloud.Common.EntityObjects;
using Epi.Cloud.Common.Metadata;
using Epi.Web.Enter.Common.DTO;

namespace Epi.Web.MVC.Utility
{
    //public class surveyResponseXML
    public class SurveyResponseDocDb
    {
        private IEnumerable<AbridgedFieldInfo> _pageFields;
        private IEnumerable<XElement> _pageFieldsXml;
        private string _requiredList = "";

        private Dictionary<string, string> _responseDetailList = new Dictionary<string, string>();

        private MetadataAccessor _metadataAccessor;
        private MetadataAccessor MetadataAccessor
        {
            get { return _metadataAccessor = _metadataAccessor ?? new MetadataAccessor(); }
        }

        public string RequiredList
        {
            get { return _requiredList; }
            set { _requiredList = value; }
        }

        public SurveyResponseDocDb(IEnumerable<AbridgedFieldInfo> pageFields, string requiredList, IEnumerable<XElement> pageFieldsXml)
        {
            _pageFields = pageFields;
            _requiredList = requiredList;
            _pageFieldsXml = pageFieldsXml;
        }
        public SurveyResponseDocDb()
        {
        }


        public void Add(MvcDynamicForms.Form pForm)
        {
            _responseDetailList.Clear();
            foreach (var field in pForm.InputFields)
            {
                if (!field.IsPlaceHolder)
                {
                    if (this._responseDetailList.ContainsKey(field.Title))
                    {
                        this._responseDetailList[field.Title] = field.Response;
                    }
                    else
                    {
                        this._responseDetailList.Add(field.Title, field.Response);
                    }
                }
            }
        }

        public void Add(MvcDynamicForms.Fields.InputField pField)
        {
            if (this._responseDetailList.ContainsKey(pField.Title))
            {
                this._responseDetailList[pField.Title] = pField.GetXML();
            }
            else
            {
                this._responseDetailList.Add(pField.Title, pField.GetXML());
            }
        }

        public void SetValue(string pKey, string pXMLValue)
        {
            if (this._responseDetailList.ContainsKey(pKey))
            {
                this._responseDetailList[pKey] = pXMLValue;
            }
            else
            {
                this._responseDetailList.Add(pKey, pXMLValue);
            }
        }


        public string GetValue(string pKey)
        {
            string result = null;

            if (this._responseDetailList.ContainsKey(pKey))
            {
                result = this._responseDetailList[pKey];
            }

            return result;
        }

        public FormResponseDetail CreateResponseDetail(string formId, bool addRoot, int currentPage, string pageId, out XmlDocument xml)
        {
            var formName = MetadataAccessor.GetFormDigest(formId).FormName;
            var formResponseDetail = new FormResponseDetail
            {
                FormId = formId,
                FormName = formName,
                LastPageVisited = currentPage == 0 ? 1 : 0
            };

            if (currentPage != 0)
            {
                var pageResponseDetail = new PageResponseDetail();
                pageResponseDetail.PageId = Convert.ToInt32(pageId);
                pageResponseDetail.PageNumber = currentPage;
                pageResponseDetail.ResponseQA = _responseDetailList;
                formResponseDetail.AddPageResponseDetail(pageResponseDetail);
            }

            #region Generate XML
            xml = new XmlDocument();
            XmlElement root = xml.CreateElement("SurveyResponse");

            if (currentPage == 0)
            {
                root.SetAttribute("SurveyId", formId);
                root.SetAttribute("LastPageVisited", "1");
                root.SetAttribute("HiddenFieldsList", "");
                root.SetAttribute("HighlightedFieldsList", "");
                root.SetAttribute("DisabledFieldsList", "");
                root.SetAttribute("RequiredFieldsList", "");

                xml.AppendChild(root);
            }

            XmlElement PageRoot = xml.CreateElement("Page");
            if (currentPage != 0)
            {
                PageRoot.SetAttribute("PageNumber", currentPage.ToString());
                //PageRoot.SetAttribute("PageId", Pageid);//Added PageId Attribute to the page node
                PageRoot.SetAttribute("PageId", currentPage.ToString());//Added PageId Attribute to the page node
                PageRoot.SetAttribute("MetaDataPageId", pageId.ToString());
                xml.AppendChild(PageRoot);
            }

            foreach (KeyValuePair<string, string> pair in this._responseDetailList)
            {
                XmlElement child = xml.CreateElement(Epi.Web.MVC.Constants.Constant.RESPONSE_DETAILS);
                child.SetAttribute("QuestionName", pair.Key);
                child.InnerText = pair.Value;
                PageRoot.AppendChild(child);
            }
            #endregion // Generate XML

            return formResponseDetail;
        }
        public int GetNumberOfPages(XDocument Xml)
        {
            var _FieldsTypeIDs = from _FieldTypeID in
                                     Xml.Descendants("View")
                                 select _FieldTypeID;

            return _FieldsTypeIDs.Elements().Count();
        }

        public FormResponseDetail CreateResponseDocument(PageDigest[] pageDigests, XDocument xmlMetadata, out string xmlResponse)
        {
            int numberOfPages = pageDigests.Length;

            #region XML
            XDocument xmlResponseDoc = new XDocument();
            int numberOfXmlPages = GetNumberOfPages(xmlMetadata);
            for (int i = 0; numberOfXmlPages > i - 1; i++)
            {
                var fieldsTypeIDs = from _FieldTypeID in
                                         xmlMetadata.Descendants("Field")
                                     where _FieldTypeID.Attribute("Position").Value == (i - 1).ToString()
                                     select _FieldTypeID;

                this._pageFieldsXml = fieldsTypeIDs;
                string pageId;
                if (this._pageFieldsXml.Count() > 0)
                {
                    pageId = this._pageFieldsXml.First().Attribute("PageId").Value;
                }
                else
                {
                    pageId = "";
                }


                if (i == 0)
                {
                    xmlResponseDoc = ToXDocument(GetResponseXml("", true, i, pageId));

                }
                else
                {
                    XDocument currentPageXml = ToXDocument(GetResponseXml("", false, i, pageId));
                    xmlResponseDoc = MergeXml(xmlResponseDoc, currentPageXml, i);
                }
            }

            xmlResponse = xmlResponseDoc.ToString();

            #endregion XML

            var firstPageDigest = pageDigests.First();
            var formId = firstPageDigest.FormId;
            var formName = firstPageDigest.FormName;

            FormResponseDetail formResponseDetail = new FormResponseDetail { FormId = formId, FormName = formName, LastPageVisited = 1 };
            foreach (var pageDigest in pageDigests)
            {
                var fieldNames = pageDigest.FieldNames;
                var pageResponseDetail = new PageResponseDetail
                {
                    PageId = pageDigest.PageId,
                    PageNumber = pageDigest.PageNumber,
                    
                    ResponseQA = fieldNames.Select(x => new { Key = x.ToLower(), Value = string.Empty }).ToDictionary(n => n.Key, v => v.Value)
                };
                formResponseDetail.AddPageResponseDetail(pageResponseDetail);
            }
            return formResponseDetail;
        }

        public XDocument ToXDocument(XmlDocument xmlDocument)
        {
            using (var nodeReader = new XmlNodeReader(xmlDocument))
            {
                nodeReader.MoveToContent();
                return XDocument.Load(nodeReader);
            }
        }
        public XDocument MergeXml(XDocument SavedXml, XDocument CurrentPageResponseXml, int Pagenumber)
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
        public XmlDocument GetResponseXml(string SurveyId, bool AddRoot, int CurrentPage, string Pageid)
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

            foreach (var field in this._pageFieldsXml)
            {
                XmlElement child = xml.CreateElement(Epi.Web.MVC.Constants.Constant.RESPONSE_DETAILS);
                child.SetAttribute("QuestionName", field.Attribute("Name").Value);
                child.InnerText = field.Value;
                PageRoot.AppendChild(child);
                //Start Adding required controls to the list
                SetRequiredListXml(field);
            }

            return xml;
        }
        public FormResponseDetail GetResponse(string formId, string formName, bool AddRoot, int CurrentPage, int PageId)
        {
            var formResponseDetail = new FormResponseDetail { FormId = formId, FormName = formName };
            if (CurrentPage == 0)
            {
                formResponseDetail.LastPageVisited = 1;
            }
            if (AddRoot)
            {
                if (CurrentPage != 0)
                {
                    var pageResponseDetail = new PageResponseDetail { PageId = PageId, PageNumber = CurrentPage };

                    foreach (var field in _pageFields ?? new AbridgedFieldInfo[0])
                    {
                        pageResponseDetail.ResponseQA.Add(field.FieldName, field.Value);
                        //Start Adding required controls to the list
                        SetRequiredList(field, _requiredList);
                    }

                    formResponseDetail.AddPageResponseDetail(pageResponseDetail);
                }
            }

            return formResponseDetail;
        }

        public string SetRequiredList(AbridgedFieldInfo field, string requiredList)
        {
            requiredList = requiredList ?? string.Empty;
            var name = field.FieldName.ToLower();
            if (field.IsRequired && !requiredList.Contains(name))
            {
                requiredList += (requiredList == "" ? "" : ",") + name; 
            }
            return requiredList;
        }

        public void SetRequiredListXml(XElement _Fields)
        {
            bool isRequired = false;
            string value = _Fields.Attribute("IsRequired").Value;

            if (bool.TryParse(value, out isRequired))
            {
                if (isRequired)
                {
                    if (!this._requiredList.Contains(_Fields.Attribute("Name").Value))
                    {
                        if (this._requiredList != "")
                        {
                            this._requiredList = this._requiredList + "," + _Fields.Attribute("Name").Value.ToLower();
                        }
                        else
                        {
                            this._requiredList = _Fields.Attribute("Name").Value.ToLower();
                        }
                    }
                }
            }
        }

        public Epi.Web.MVC.Models.ResponseModel ConvertResponseDetailToModel(SurveyAnswerDTO item, List<KeyValuePair<int, string>> Columns)
        {
            Epi.Web.MVC.Models.ResponseModel ResponseModel = new Models.ResponseModel();


            var MetaDataColumns = Epi.Web.MVC.Constants.Constant.MetaDaTaColumnNames();

            try
            {
                ResponseModel.Column0 = item.ResponseId;
                ResponseModel.IsLocked = item.IsLocked;

                var responseQA = item.ResponseDetail.FlattenedResponseQA(key => key.ToLower());
                string value;
                var columnsCount = Columns.Count;
                for (int i = 0; i < 5; ++i)
                {
                    if (i >= columnsCount)
                    {
                        // set value to empty string for unspecified columns
                        value = string.Empty;
                    }
                    else if (MetaDataColumns.Contains(Columns[i].Value))
                    {
                        // set value to value of special column
                        value = GetColumnValue(item, Columns[i].Value);
                    }
                    else
                    {
                        // set value to value in the response
                        value = responseQA.TryGetValue(Columns[i].Value.ToLower(), out value) ? (value ?? string.Empty) : string.Empty;
                    }

                    // set the associated ResponseModel column
                    switch (i)
                    {
                        case 0:
                            ResponseModel.Column1 = value;
                            break;
                        case 1:
                            ResponseModel.Column2 = value;
                            break;
                        case 2:
                            ResponseModel.Column3 = value;
                            break;
                        case 3:
                            ResponseModel.Column4 = value;
                            break;
                        case 4:
                            ResponseModel.Column5 = value;
                            break;
                    }
                }

                return ResponseModel;
            }
            catch (Exception Ex)
            {

                throw new Exception(Ex.Message);
            }
        }
        private string GetColumnValue(Epi.Web.Enter.Common.DTO.SurveyAnswerDTO item, string columnName)
        {
            string ColumnValue = "";
            switch (columnName)
            {
                case "_UserEmail":
                    ColumnValue = item.UserEmail;
                    break;
                case "_DateUpdated":
                    ColumnValue = item.DateUpdated.ToString();
                    break;
                case "_DateCreated":
                    ColumnValue = item.DateCreated.ToString();
                    break;
                case "IsDraftMode":
                case "_Mode":
                    if (item.IsDraftMode.ToString().ToUpper() == "TRUE")
                    {
                        ColumnValue = "Staging";
                    }
                    else
                    {
                        ColumnValue = "Production";

                    }
                    break;
            }
            return ColumnValue;
        }
    }
}
