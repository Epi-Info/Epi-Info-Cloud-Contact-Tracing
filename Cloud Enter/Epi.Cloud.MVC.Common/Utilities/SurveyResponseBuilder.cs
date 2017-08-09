using System;
using System.Collections.Generic;
using System.Linq;
using Epi.Cloud.Common.Extensions;
using Epi.Cloud.Common.Message;
using Epi.Cloud.Common.Metadata;
using Epi.Common.Core.Interfaces;
using Epi.DataPersistence.Constants;
using Epi.DataPersistence.DataStructures;
using Epi.FormMetadata.DataStructures;

namespace Epi.Cloud.MVC.Utility
{
    public class SurveyResponseBuilder
    {
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

        public SurveyResponseBuilder(string requiredList)
        {
            _requiredList = requiredList;
        }

        public SurveyResponseBuilder()
        {
        }

        public void Add(MvcDynamicForms.Form pForm)
        {
            _responseQA.Clear();
            foreach (var field in pForm.InputFields)
            {
                if (!field.IsPlaceHolder)
                {
                    _responseQA[field.Title] = field.Response;
                }
            }
        }

        /// <summary>
        /// Deprecated
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="addRoot"></param>
        /// <param name="currentPage"></param>
        /// <param name="pageId"></param>
        /// <param name="responseId"></param>
        /// <returns></returns>
        public FormResponseDetail UpdateResponseDetail(FormResponseDetail formResponseDetail, int currentPage, string pageId)
        {
            if (formResponseDetail.PageResponseDetailList.Count() == 0)
            {
                formResponseDetail.IsNewRecord = true;
                formResponseDetail.RecStatus = RecordStatus.InProcess;
                formResponseDetail.LastPageVisited = currentPage == 0 ? 1 : currentPage;
            }
            if (!String.IsNullOrWhiteSpace(pageId))
            {
                var pageResponseDetail = formResponseDetail.PageResponseDetailList.SingleOrDefault(p => p.PageId == Convert.ToInt32(pageId));
                if (pageResponseDetail == null)
                {
                    pageResponseDetail = new PageResponseDetail
                    {
                        PageId = Convert.ToInt32(pageId),
                        PageNumber = currentPage,
                        ResponseQA = _responseQA
                    };
                    formResponseDetail.AddPageResponseDetail(pageResponseDetail);
                }
                else
                {
                    pageResponseDetail.ResponseQA = _responseQA;
                }
            }

            return formResponseDetail;
        }

        public FormResponseDetail CreateResponseDocument(IResponseContext responseContext, PageDigest[] pageDigests)
        {
            int numberOfPages = pageDigests.Length;

            var firstPageDigest = pageDigests.First();
            var formId = firstPageDigest.FormId;
            var formName = firstPageDigest.FormName;

            FormResponseDetail formResponseDetail = responseContext.ToFormResponseDetail();
            formResponseDetail.LastPageVisited = 1;

            foreach (var pageDigest in pageDigests)
            {
                var fieldNames = pageDigest.FieldNames;
                var pageResponseDetail = new PageResponseDetail
                {
                    PageId = pageDigest.PageId,
                    PageNumber = pageDigest.PageNumber,
                    
                    ResponseQA = fieldNames.Select(x => new { Key = x.ToLower(), Value = string.Empty }).ToDictionary(n => n.Key, v => v.Value)
                };
                SetRequiredList(pageDigest.Fields);
                formResponseDetail.AddPageResponseDetail(pageResponseDetail);
            }
            return formResponseDetail;
        }


        public void SetRequiredList(AbridgedFieldInfo[] fields)
        {
            foreach (var field in fields)
            {
                if (field.IsRequired == true)
                {
                    if (this.RequiredList != "")
                    {
                        this.RequiredList = this.RequiredList + "," + field.FieldName.ToLower();
                    }
                    else
                    {
                        this.RequiredList = field.FieldName.ToLower();
                    }
                }
            } 
        }

    }
}
