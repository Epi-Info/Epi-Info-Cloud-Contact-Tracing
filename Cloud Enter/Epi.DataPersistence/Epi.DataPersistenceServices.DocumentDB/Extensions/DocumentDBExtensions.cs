using System.Collections.Generic;
using System.Linq;
using Epi.Cloud.Common.Extensions;
using Epi.Cloud.Common.Metadata;
using Epi.Common.Core.DataStructures;
using Epi.DataPersistence.DataStructures;
using Epi.DataPersistenceServices.DocumentDB.FormSettings;
using Epi.PersistenceServices.DocumentDB;

namespace Epi.DataPersistence.Extensions
{
    public static class DocumentDBExtensions
    {
        static MetadataAccessor _metadataAccessor = new MetadataAccessor();

        static DocumentDBExtensions()
        {
        }

        /// <summary>
        /// ToHierarchicalFormResponseDetail
        /// </summary>
        /// <param name="formResponseProperties"></param>
        /// <param name="formResponseResource"></param>
        /// <returns></returns>
        public static FormResponseDetail ToHierarchicalFormResponseDetail(this FormResponseProperties formResponseProperties, FormResponseResource formResponseResource)
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

            target.OrgId = source.OrgId;
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

                OrgId = formResponseProperties.OrgId,
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

        public static FormResponseDetail ToHierarchicalFormResponseDetail(this IEnumerable<FormResponseProperties> formResponsePropertiesList)
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

        public static FormResponseProperties ToFormResponseProperties(this FormResponseDetail formResponseDetail)
        {
            if (formResponseDetail == null) return null;

            FormResponseProperties formResponseProperties = new FormResponseProperties
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

                IsNewRecord = formResponseDetail.IsNewRecord,

                RecStatus = formResponseDetail.RecStatus,
                LastPageVisited = formResponseDetail.LastPageVisited,

                FirstSaveLogonName = formResponseDetail.FirstSaveLogonName,
                LastSaveLogonName = formResponseDetail.LastSaveLogonName,
                FirstSaveTime = formResponseDetail.FirstSaveTime,
                LastSaveTime = formResponseDetail.LastSaveTime,

                OrgId = formResponseDetail.OrgId,
                UserId = formResponseDetail.UserId,
                UserName = formResponseDetail.UserName,

                IsDraftMode = formResponseDetail.IsDraftMode,
                IsLocked = formResponseDetail.IsLocked,
                RequiredFieldsList = formResponseDetail.RequiredFieldsList,
                HiddenFieldsList = formResponseDetail.HiddenFieldsList,
                HighlightedFieldsList = formResponseDetail.HighlightedFieldsList,
                DisabledFieldsList = formResponseDetail.DisabledFieldsList,
                ResponseQA = formResponseDetail.FlattenedResponseQA()
            };

            return formResponseProperties;
        }

        public static List<FormResponseProperties> ToFormResponsePropertiesFlattenedList(this FormResponseDetail formResponseDetail)
        {
            List<FormResponseProperties> formResponsePropertiesList = new List<FormResponseProperties>();
            var formResponseProperties = formResponseDetail.ToFormResponseProperties();
            formResponsePropertiesList.Add(formResponseProperties);
            foreach (var childFormResponseDetail in formResponseDetail.ChildFormResponseDetailList)
            {
                var childFormResponseProperties = childFormResponseDetail.ToFormResponseProperties();
                formResponsePropertiesList.Add(childFormResponseProperties);
            }
            return formResponsePropertiesList;
        }

        public static List<FormResponseDetail> ToFormResponseDetailList(this IEnumerable<FormResponseProperties> formResponsePropertiesList)
        {
            List<FormResponseDetail> formResonseDetailList = formResponsePropertiesList.Select(p => p.ToFormResponseDetail()).ToList();
            return formResonseDetailList;
        }

        public static List<FormSettings> ToFormSettingsList(this IEnumerable<FormSettingsProperties> formSettingsPropertiesList)
        {
            var formSettingsList = formSettingsPropertiesList.Select(f => f.ToFormSettings()).ToList();
            return formSettingsList;
        }

        public static FormSettings ToFormSettings(this FormSettingsProperties formSettingsProperties)
        {
            var formSettings = new FormSettings
            {
                FormId = formSettingsProperties.FormId,
                FormName = formSettingsProperties.FormName,
                IsDisabled = formSettingsProperties.IsDisabled,
                IsDraftMode = formSettingsProperties.IsDraftMode,
                IsShareable = formSettingsProperties.IsShareable,
                DataAccessRuleId = formSettingsProperties.DataAccessRuleId,
                ResponseDisplaySettings = formSettingsProperties.ToResponseDisplaySettingsList()
            };
            return formSettings;
        }

        public static FormSettingsProperties ToFormSettingsProperties(this FormSettings formSettings, FormSettingsProperties formSettingsProperties)
        {
            if (formSettingsProperties == null) formSettingsProperties = new FormSettingsProperties();
            formSettingsProperties.FormId = formSettings.FormId;
            formSettingsProperties.FormName = formSettings.FormName;
            formSettingsProperties.IsDisabled = formSettings.IsDisabled;
            formSettingsProperties.IsDraftMode = formSettings.IsDraftMode;
            formSettingsProperties.IsShareable = formSettings.IsShareable;
            formSettingsProperties.DataAccessRuleId = formSettings.DataAccessRuleId;
            if (formSettings.ResponseDisplaySettings != null && formSettings.ResponseDisplaySettings.Count > 0)
            {
                formSettingsProperties.ColumnNames = formSettings.ResponseDisplaySettings.Select(r => r.ColumnName).ToList();
            }
            return formSettingsProperties;
        }

        public static List<ResponseGridColumnSettings> ToResponseDisplaySettingsList(this FormSettingsProperties formSettingsProperties)
        {
            List<ResponseGridColumnSettings> responseDisplaySettingsList = new List<ResponseGridColumnSettings>();
            int sortOrder = 0;
            foreach (var columnName in formSettingsProperties.ColumnNames)
            {
                responseDisplaySettingsList.Add(
                 new ResponseGridColumnSettings
                 {
                     FormId = formSettingsProperties.FormId,
                     ColumnName = columnName,
                     SortOrder = ++sortOrder
                 });
            }
            return responseDisplaySettingsList;
        }


        public static List<string> ToResponseDisplaySettingsList(this List<ResponseGridColumnSettings> responseDisplaySettingsList)
        {
            responseDisplaySettingsList = responseDisplaySettingsList.OrderBy(s => s.SortOrder).ToList();
            var responseDisplaySettingsPropertiesList = responseDisplaySettingsList.Select(s => s.ColumnName).ToList();
            return responseDisplaySettingsPropertiesList;
        }
    }
}
