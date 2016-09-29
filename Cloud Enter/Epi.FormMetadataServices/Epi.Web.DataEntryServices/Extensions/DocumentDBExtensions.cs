﻿using System;
using System.Collections.Generic;
using Epi.Cloud.DataEntryServices.Model;
using Epi.DataPersistence.DataStructures;

namespace Epi.Cloud.DataEntryServices.Extensions
{
    public static class DocumentDBExtensions
    {
        public static PageResponseDetail ToPageResponseDetail(this Epi.PersistenceServices.DocumentDB.DataStructures.PageResponseProperties pageResponseProperties, FormResponseDetail formResponseDetail)
        {
            return pageResponseProperties.ToPageResponseDetail(formResponseDetail.FormId, formResponseDetail.FormName);
        }

        public static PageResponseDetail ToPageResponseDetail(this Epi.PersistenceServices.DocumentDB.DataStructures.PageResponseProperties pageResponseProperties, string formId, string formName)
        {
            var pageResponseDetail = new PageResponseDetail
            {
                GlobalRecordID = pageResponseProperties.GlobalRecordID,
                FormId = formId,
                FormName = formName,
                PageId = pageResponseProperties.PageId,
                ResponseQA = pageResponseProperties.ResponseQA
            };
            return pageResponseDetail;
        }

        public static FormResponseDetail ToFormResponseDetail(this Epi.PersistenceServices.DocumentDB.DataStructures.HierarchicalDocumentResponseProperties hierarchicalDocumentResponseProperties)
        {
            var formResponseProperties = hierarchicalDocumentResponseProperties.FormResponseProperties;
            var pageResponsePropertiesList = hierarchicalDocumentResponseProperties.PageResponsePropertiesList;

            var formResponseDetail = formResponseProperties.ToFormResponseDetail(pageResponsePropertiesList);
            formResponseDetail.AddFormResponseDetailChildren(hierarchicalDocumentResponseProperties);

            return formResponseDetail;
        }

        private static void AddFormResponseDetailChildren(this FormResponseDetail formResponseDetail, Epi.PersistenceServices.DocumentDB.DataStructures.HierarchicalDocumentResponseProperties hierarchicalDocumentResponseProperties)
        {
            foreach (var childHierarchicalDocumentResponseProperties in hierarchicalDocumentResponseProperties.ChildResponseList)
            {
                var childFormResponseDetail = childHierarchicalDocumentResponseProperties.ToFormResponseDetail();
                formResponseDetail.AddChildFormResponseDetail(childFormResponseDetail);
            }
        }

        public static FormResponseDetail ToFormResponseDetail(this Epi.PersistenceServices.DocumentDB.DataStructures.DocumentResponseProperties documentResponseProperties)
        {
            FormResponseDetail formResponseDetail = null;

            var formResponseProperties = documentResponseProperties.FormResponseProperties;
            if (formResponseProperties != null)
            {
                formResponseDetail = formResponseProperties.ToFormResponseDetail();
                if (documentResponseProperties.PageResponsePropertiesList != null)
                {
                    foreach (var pageResponseProperties in documentResponseProperties.PageResponsePropertiesList)
                    {
                        var pageResponseDetail = pageResponseProperties.ToPageResponseDetail(formResponseDetail);
                        formResponseDetail.AddPageResponseDetail(pageResponseDetail);
                    }
                }
            }

            return formResponseDetail;
        }

        public static FormResponseDetail ToFormResponseDetail(this Epi.PersistenceServices.DocumentDB.DataStructures.FormResponseProperties formResponseProperties, List<Epi.PersistenceServices.DocumentDB.DataStructures.PageResponseProperties> pageResponsePropertiesList = null)
        {
            var formResponseDetail = new FormResponseDetail
            {
                GlobalRecordID = formResponseProperties.GlobalRecordID,

                FormId = formResponseProperties.FormId,
                FormName = formResponseProperties.FormName,
                RecStatus = formResponseProperties.RecStatus,
                RelateParentResponseId = formResponseProperties.RelateParentId,
                FirstSaveLogonName = formResponseProperties.FirstSaveLogonName,
                LastSaveLogonName = formResponseProperties.LastSaveLogonName,
                FirstSaveTime = formResponseProperties.FirstSaveTime,
                LastSaveTime = formResponseProperties.LastSaveTime,

                LastActiveUserId = formResponseProperties.UserId,
                IsRelatedView = formResponseProperties.IsRelatedView,
                IsDraftMode = formResponseProperties.IsDraftMode,
                PageIds = formResponseProperties.PageIds
            };

            if (pageResponsePropertiesList != null && pageResponsePropertiesList.Count > 0)
            {
                foreach (var pageResponseProperties in pageResponsePropertiesList)
                {
					if (pageResponseProperties != null)
					{
						formResponseDetail.AddPageResponseDetail(pageResponseProperties.ToPageResponseDetail(formResponseDetail));
					}
                }
            }

            return formResponseDetail;
        }

		public static Epi.PersistenceServices.DocumentDB.DataStructures.FormResponseProperties ToFormResponseProperties(FormResponseDetail formResponseDetail)
		{
			var formResponseProperties = new Epi.PersistenceServices.DocumentDB.DataStructures.FormResponseProperties
			{
				GlobalRecordID = formResponseDetail.GlobalRecordID,

				FormId = formResponseDetail.FormId,
				FormName = formResponseDetail.FormName,
				RecStatus = formResponseDetail.RecStatus,
				RelateParentId = formResponseDetail.RelateParentResponseId,
				FirstSaveLogonName = formResponseDetail.FirstSaveLogonName,
				LastSaveLogonName = formResponseDetail.LastSaveLogonName,
				FirstSaveTime = formResponseDetail.FirstSaveTime,
				LastSaveTime = formResponseDetail.LastSaveTime,

				UserId = formResponseDetail.LastActiveUserId,
				IsRelatedView = formResponseDetail.IsRelatedView,
				IsDraftMode = formResponseDetail.IsDraftMode,
				PageIds = formResponseDetail.PageIds
			};
			return formResponseProperties;
		}

		public static Epi.PersistenceServices.DocumentDB.DataStructures.PageResponseProperties ToPageResponseProperties(this PageResponseDetail pageResponseDetail)
		{
			var pageResponseProperties = new Epi.PersistenceServices.DocumentDB.DataStructures.PageResponseProperties
			{
				GlobalRecordID = pageResponseDetail.GlobalRecordID,
				PageId = pageResponseDetail.PageId,
				ResponseQA = pageResponseDetail.ResponseQA
			};
			return pageResponseProperties;
		}
    }
}
