using System;
using System.Collections.Generic;
using System.Linq;
using Epi.Cloud.Common.Metadata;

namespace Epi.Cloud.Common.EntityObjects
{
    public partial class FormResponseDetail
    {
        private MetadataAccessor _metadataAccessor;

        public FormResponseDetail()
        {
            RequiredFieldsList = string.Empty;
            HiddenFieldsList = string.Empty;
            HighlightedFieldsList = string.Empty;
            DisabledFieldsList = string.Empty;
            PageResponseDetailList = new List<PageResponseDetail>();
            ChildFormResponseDetailList = new List<FormResponseDetail>();
        }

        public void AddPageResponseDetail(PageResponseDetail pageResponseDetail)
        {
            var existingItem = PageResponseDetailList.SingleOrDefault(p => p.PageId == pageResponseDetail.PageId);
            if (existingItem != null) PageResponseDetailList.Remove(existingItem);
            FormId = FormId ?? pageResponseDetail.FormId;
            FormName = FormName ?? pageResponseDetail.FormName;
            pageResponseDetail.FormId = FormId;
            pageResponseDetail.FormName = FormName;
            if (pageResponseDetail.PageNumber < 1)
            {
                if (_metadataAccessor == null) _metadataAccessor = new MetadataAccessor(FormId);
                var pageDigest = _metadataAccessor.GetPageDigestByPageId(FormId, pageResponseDetail.PageId);
                pageResponseDetail.PageNumber = pageDigest.PageNumber;
            }
            PageResponseDetailList.Add(pageResponseDetail);
        }

        public void AddChildFormResponseDetail(FormResponseDetail childFormResponseDetail)
        {
            var existingItem = ChildFormResponseDetailList.SingleOrDefault(f => f.FormId == childFormResponseDetail.FormId);
            if (existingItem != null) ChildFormResponseDetailList.Remove(childFormResponseDetail);
            childFormResponseDetail.RelateParentId = FormId;
            ChildFormResponseDetailList.Add(childFormResponseDetail);
        }

        public Dictionary<string, string> FlattenedResponseQA(Func<string, string> keyModifier = null)
        {
            var flattenedResponseQA = new Dictionary<string, string>();
            foreach (var pageResponseDetail in PageResponseDetailList)
            {
                foreach (var qa in pageResponseDetail.ResponseQA)
                    flattenedResponseQA[keyModifier != null ? keyModifier(qa.Key) : qa.Key] = qa.Value;
            }
            return flattenedResponseQA;
        }

        public PageResponseDetail GetPageResponseDetailByPageId(int pageId)
        {
            var pageResponseDetail = PageResponseDetailList.SingleOrDefault(p => p.PageId == pageId);
            return pageResponseDetail;
        }
        public PageResponseDetail GetPageResponseDetailByPageNumber(int pageNumber)
        {
            var pageResponseDetail = PageResponseDetailList.SingleOrDefault(p => p.PageNumber == pageNumber);
            return pageResponseDetail;
        }
    }
}
