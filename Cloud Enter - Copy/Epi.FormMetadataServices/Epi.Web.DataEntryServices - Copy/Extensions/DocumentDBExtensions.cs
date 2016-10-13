using System;
using Epi.Cloud.Common.EntityObjects;
using Epi.Cloud.DataEntryServices.Model;

namespace Epi.Cloud.DataEntryServices.Extensions
{
    public static class DocumentDBExtensions
    {
        public static PageResponseDetail ToPageResponseDetail(this PageResponseDetailResource pageResponseDetailResource)
        {
            var pageResponseDetail = new PageResponseDetail
            {
                GlobalRecordID = pageResponseDetailResource.GlobalRecordID,
                PageId = pageResponseDetailResource.PageId,
                ResponseQA = pageResponseDetailResource.ResponseQA
            };
            return pageResponseDetail;
        }

        public static FormResponseDetail ToFormResponseDetail(this PageResponseDetailResource pageResponseDetailResource, string formId = null, string formName = null, string parentFormId = null)
        {
            var formResponseDetail = new FormResponseDetail
            {
                GlobalRecordID = pageResponseDetailResource.GlobalRecordID,
                FormId = formId,
                ParentFormId = parentFormId
            };
            var pageResponseDetail = pageResponseDetailResource.ToPageResponseDetail();
            formResponseDetail.PageResponseDetailList.Add(pageResponseDetail);
            return formResponseDetail;
        }
    }
}
