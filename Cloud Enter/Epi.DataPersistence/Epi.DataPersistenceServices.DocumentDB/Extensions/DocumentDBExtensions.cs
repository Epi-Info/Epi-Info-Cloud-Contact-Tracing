using System.Collections.Generic;
using System.Linq;
using Epi.Cloud.Common.Extensions;
using Epi.Cloud.Common.Metadata;
using Epi.DataPersistence.Constants;
using Epi.DataPersistence.DataStructures;
using Epi.PersistenceServices.DocumentDB;

namespace Epi.DataPersistence.Extensions
{
    public static class DocumentDBExtensions
    {
        static MetadataAccessor _metadataAccessor = new MetadataAccessor();

        static DocumentDBExtensions()
        {
        }

        public static FormResponseDetail ToFormResponseDetail(this FormResponseResource formResponseResource)
        {
            var formResponseProperties = formResponseResource.FormResponseProperties;
            var formResponseDetail = formResponseProperties.ToFormResponseDetail();
            return formResponseDetail;
        }

        public static FormResponseDetail ToFormResponseDetail(this DocumentResponseProperties documentResponseProperties)
        {
            FormResponseDetail formResponseDetail = null;

            var formResponseProperties = documentResponseProperties.FormResponseProperties;
            if (formResponseProperties != null)
            {
                formResponseDetail = formResponseProperties.ToFormResponseDetail();
            }

            return formResponseDetail;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="formResponseProperties"></param>
        /// <param name="formResponseResource"></param>
        /// <returns></returns>
        public static FormResponseDetail ToHierarchialFormResponseDetail(this FormResponseProperties formResponseProperties, FormResponseResource formResponseResource)
        {
            FormResponseDetail formResponseDetail = formResponseProperties.ToFormResponseDetail();
            List<FormResponseDetail> flattened = formResponseDetail.FlattenHierarchy();
            formResponseResource.CascadeThroughChildren(formResponseProperties,
                frp =>
                {
                    var frd = frp.ToFormResponseDetail();
                    var parent = flattened.Where(f => f.ResponseId == frp.ParentResponseId).SingleOrDefault();
                    if (parent != null)
                    {
                        parent.ChildFormResponseDetailList.Add(frd);
                    }
                    else
                    {
                        throw new KeyNotFoundException();
                    }
                    flattened.Add(frd);
                });
            return formResponseDetail;
        }

        public static FormResponseProperties CopyTo(this FormResponseProperties source, FormResponseProperties target)
        {
            target.ResponseId = source.ResponseId;
            target.FormId = source.FormId;
            target.FormName = source.FormName;

            target.ParentResponseId = source.ParentResponseId;
            target.ParentFormId = source.ParentFormId;
            target.ParentFormName = source.ParentFormName;

            target.RootResponseId = source.RootResponseId;
            target.RootFormId = source.RootFormId;
            target.RootFormName = source.RootFormName;

            target.IsNewRecord = source.IsNewRecord;

            target.RecStatus = source.RecStatus;
            target.LastPageVisited = source.LastPageVisited;

            target.FirstSaveLogonName = source.FirstSaveLogonName;
            target.LastSaveLogonName = source.LastSaveLogonName;
            target.FirstSaveTime = source.FirstSaveTime;
            target.LastSaveTime = source.LastSaveTime;

            target.UserId = source.UserId;
            target.UserName = source.UserName;

            target.IsDraftMode = source.IsDraftMode;
            target.IsLocked = source.IsLocked;
            target.RequiredFieldsList = source.RequiredFieldsList;
            target.HiddenFieldsList = source.HiddenFieldsList;
            target.HighlightedFieldsList = source.HighlightedFieldsList;
            target.DisabledFieldsList = source.DisabledFieldsList;

            target.ResponseQA = source.ResponseQA;
            return target;
        }

        public static FormResponseDetail ToFormResponseDetail(this FormResponseProperties formResponseProperties)
        {
            if (formResponseProperties == null) return null;

            FormResponseDetail formResponseDetail = new FormResponseDetail
            {
                ResponseId = formResponseProperties.ResponseId,
                FormId = formResponseProperties.FormId,
                FormName = formResponseProperties.FormName,

                ParentResponseId = formResponseProperties.ParentResponseId,
                ParentFormId = formResponseProperties.ParentFormId,
                ParentFormName = formResponseProperties.ParentFormName,

                RootResponseId = formResponseProperties.RootResponseId,
                RootFormId = formResponseProperties.RootFormId,
                RootFormName = formResponseProperties.RootFormName,

                IsNewRecord = formResponseProperties.IsNewRecord,

                RecStatus = formResponseProperties.RecStatus,
                LastPageVisited = formResponseProperties.LastPageVisited,

                FirstSaveLogonName = formResponseProperties.FirstSaveLogonName,
                LastSaveLogonName = formResponseProperties.LastSaveLogonName,
                FirstSaveTime = formResponseProperties.FirstSaveTime,
                LastSaveTime = formResponseProperties.LastSaveTime,

                UserId = formResponseProperties.UserId,
                UserName = formResponseProperties.UserName,

                IsRelatedView = formResponseProperties.IsRelatedView,
                IsDraftMode = formResponseProperties.IsDraftMode,
                IsLocked = formResponseProperties.IsLocked,
                RequiredFieldsList = formResponseProperties.RequiredFieldsList,
                HiddenFieldsList = formResponseProperties.HiddenFieldsList,
                HighlightedFieldsList = formResponseProperties.HighlightedFieldsList,
                DisabledFieldsList = formResponseProperties.DisabledFieldsList,
            }.ResolveMetadataDependencies() as FormResponseDetail;

            var formDigest = _metadataAccessor.GetFormDigest(formResponseProperties.FormId);
            foreach (var qaFieldKVP in formResponseProperties.ResponseQA)
            {
                var fieldName = qaFieldKVP.Key;
                var pageId = formDigest.FieldNameToPageIdDirectory[fieldName];
                var pageResponseDetail = formResponseDetail
                    .PageResponseDetailList
                    .Where(prd => prd.PageId == pageId)
                    .SingleOrDefault() ?? new PageResponseDetail
                    {
                        PageId = pageId,
                        PageNumber = _metadataAccessor.GetPageDigestByPageId(formDigest.FormId, pageId).PageNumber,
                        FormId = formDigest.FormId,
                        FormName = formDigest.FormName,
                        ResponseId = formResponseProperties.ResponseId,
                    };
                pageResponseDetail.ResponseQA[fieldName] = qaFieldKVP.Value;
                formResponseDetail.AddPageResponseDetail(pageResponseDetail);
            }

            return formResponseDetail;
        }

        public static FormResponseDetail ToHierarchialFormResponseDetail(this IEnumerable<FormResponseProperties> formResponsePropertiesList)
        {
            FormResponseDetail formResponseDetail = null;
            if (formResponsePropertiesList != null)
            {
                foreach (var formResponseProperties in formResponsePropertiesList)
                {
                    if (formResponseDetail == null)
                    {
                        // verify that the first entry is the root response.
                        if (formResponseProperties.IsRootResponse)
                        {
                            formResponseDetail = formResponseProperties.ToFormResponseDetail();
                        }
                        else
                        {
                            // break out if the first entry is not the root response
                            break;
                        }
                    }
                    else
                    {
                        var parentFormResponseDetail = formResponseDetail.FindParentFormResponseDetail(formResponseProperties.ParentResponseId);
                        if (parentFormResponseDetail != null)
                        {
                            parentFormResponseDetail.ChildFormResponseDetailList.Add(formResponseProperties.ToFormResponseDetail());
                        }
                    }
                }
            }
            return formResponseDetail;    
        }

        public static List<FormResponseDetail> ToFormResponseDetailList(this IEnumerable<FormResponseProperties> formResponsePropertiesList)
        {
            List<FormResponseDetail> formResonseDetailList = formResponsePropertiesList.Select(p => p.ToFormResponseDetail()).ToList();
            return formResonseDetailList;
        }

        public static List<PageResponseDetail> ToPageResponseDetailList(this FormResponseProperties formResponseProperties, List<PageResponseDetail> pageResponseDetailList)
        {
            var responseId = formResponseProperties.ResponseId;
            var formId = formResponseProperties.FormId;
            var formName = formResponseProperties.FormName;
            var responseQA = formResponseProperties.ResponseQA;
            var formDigest = _metadataAccessor.GetFormDigest(formId);
            PageResponseDetail pageResponseDetail = null;
            foreach (var kvp in responseQA)
            {
                var pageId = formDigest.FieldNameToPageId(kvp.Key);
                pageResponseDetail = pageResponseDetailList.Where(p => p.PageId == pageId).SingleOrDefault();
                if (pageResponseDetail == null)
                { 
                    pageResponseDetail = new PageResponseDetail();
                    pageResponseDetailList.Add(pageResponseDetail);
                    pageResponseDetail.ResponseId = responseId;
                    pageResponseDetail.FormId = formId;
                    pageResponseDetail.FormName = formName;
                    pageResponseDetail.PageId = pageId;
                    var pageDigest = _metadataAccessor.GetPageDigestByPageId(formId, pageId);
                    pageResponseDetail.PageNumber = pageDigest.PageNumber;
                }
                pageResponseDetail.ResponseQA[kvp.Key] = kvp.Value;
            }
            return pageResponseDetailList;
        }

        public static FormResponseResource ToFormResponseResource(this FormResponseDetail formResponseDetail)
        {
            var formResponseResource = new FormResponseResource
            {
                Id = formResponseDetail.ResponseId,
                FormResponseProperties = formResponseDetail.ToFormResponseProperties()
            };
            return formResponseResource;
        }

        public static FormResponseProperties ToFormResponseProperties(this FormResponseDetail formResponseDetail)
        {
            var formResponseProperties = new FormResponseProperties
            {
                ResponseId = formResponseDetail.ResponseId,
                FormId = formResponseDetail.FormId,
                FormName = formResponseDetail.FormName,

                ParentResponseId = formResponseDetail.ParentResponseId,
                ParentFormId = formResponseDetail.ParentFormId,
                ParentFormName = formResponseDetail.ParentFormName,

                RootResponseId = formResponseDetail.RootResponseId,
                RootFormId = formResponseDetail.RootFormId,
                RootFormName = formResponseDetail.RootFormName,

                IsNewRecord = formResponseDetail.RecStatus == RecordStatus.InProcess ? formResponseDetail.IsNewRecord : false,

                RecStatus = formResponseDetail.RecStatus,
                LastPageVisited = formResponseDetail.LastPageVisited,
                FirstSaveLogonName = formResponseDetail.FirstSaveLogonName,
                LastSaveLogonName = formResponseDetail.LastSaveLogonName,
                FirstSaveTime = formResponseDetail.FirstSaveTime,
                LastSaveTime = formResponseDetail.LastSaveTime,

                UserId = formResponseDetail.UserId,
                UserName = formResponseDetail.UserName,

                IsDraftMode = formResponseDetail.IsDraftMode,
                IsLocked = formResponseDetail.IsLocked,

                HiddenFieldsList = formResponseDetail.HiddenFieldsList,
                HighlightedFieldsList = formResponseDetail.HighlightedFieldsList,
                DisabledFieldsList = formResponseDetail.DisabledFieldsList,
                RequiredFieldsList = formResponseDetail.RequiredFieldsList,

                ResponseQA = formResponseDetail.FlattenedResponseQA(key => key.ToLowerInvariant()),
            };

            return formResponseProperties;
        }
    }
}
