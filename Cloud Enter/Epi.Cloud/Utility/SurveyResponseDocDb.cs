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
    public class SurveyResponseDocDb
    {
        private IEnumerable<AbridgedFieldInfo> _pageFields;
        private string _requiredList = "";

        private Dictionary<string, string> _responseQA = new Dictionary<string, string>();

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

        public SurveyResponseDocDb(IEnumerable<AbridgedFieldInfo> pageFields, string requiredList)
        {
            _pageFields = pageFields;
            _requiredList = requiredList;
        }
        public SurveyResponseDocDb()
        {
        }


        public void Add(MvcDynamicForms.Form pForm)
        {
            _responseQA.Clear();
            foreach (var field in pForm.InputFields)
            {
                if (!field.IsPlaceHolder)
                {
                    if (this._responseQA.ContainsKey(field.Title))
                    {
                        this._responseQA[field.Title] = field.Response;
                    }
                    else
                    {
                        this._responseQA.Add(field.Title, field.Response);
                    }
                }
            }
        }

        public void Add(MvcDynamicForms.Fields.InputField pField)
        {
            if (this._responseQA.ContainsKey(pField.Title))
            {
                this._responseQA[pField.Title] = pField.GetXML();
            }
            else
            {
                this._responseQA.Add(pField.Title, pField.GetXML());
            }
        }

        public void SetValue(string pKey, string pXMLValue)
        {
            if (this._responseQA.ContainsKey(pKey))
            {
                this._responseQA[pKey] = pXMLValue;
            }
            else
            {
                this._responseQA.Add(pKey, pXMLValue);
            }
        }


        public string GetValue(string pKey)
        {
            string result = null;

            if (this._responseQA.ContainsKey(pKey))
            {
                result = this._responseQA[pKey];
            }

            return result;
        }

        public FormResponseDetail CreateResponseDetail(string formId, bool addRoot, int currentPage, string pageId)
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
                pageResponseDetail.ResponseQA = _responseQA;
                formResponseDetail.AddPageResponseDetail(pageResponseDetail);
            }

            return formResponseDetail;
        }
        public int GetNumberOfPages(XDocument Xml)
        {
            var _FieldsTypeIDs = from _FieldTypeID in
                                     Xml.Descendants("View")
                                 select _FieldTypeID;

            return _FieldsTypeIDs.Elements().Count();
        }

        public FormResponseDetail CreateResponseDocument(PageDigest[] pageDigests)
        {
            int numberOfPages = pageDigests.Length;

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
    }
}
