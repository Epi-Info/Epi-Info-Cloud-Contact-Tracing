﻿using System.Collections.Generic;
using System.Linq;
using Epi.DataPersistence.Constants;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.DTO;
using Epi.Common.Core.DataStructures;
using Epi.Cloud.Common.Metadata;
using System;

namespace Epi.Cloud.Common.Extensions
{
    public static class SurveyAnswerDTOExtensions
    {
        public static SurveyResponseBO ToSurveyResponseBO(this SurveyAnswerDTO surveyAnswerDTO, int? userId = null)
        {
            SurveyResponseBO surveyResponseBO = new SurveyResponseBO();
            surveyResponseBO.ResponseDetail = surveyAnswerDTO.ResponseDetail;
            surveyResponseBO.FormId = surveyAnswerDTO.SurveyId;
            surveyResponseBO.ResponseId = surveyAnswerDTO.ResponseId;
            surveyResponseBO.ParentResponseId = surveyAnswerDTO.ParentResponseId;
            surveyResponseBO.Status = surveyAnswerDTO.Status;
            surveyResponseBO.ReasonForStatusChange = surveyAnswerDTO.ReasonForStatusChange;
            surveyResponseBO.UserPublishKey = surveyAnswerDTO.UserPublishKey;
            surveyResponseBO.DateUpdated = surveyAnswerDTO.DateUpdated;
            surveyResponseBO.DateCompleted = surveyAnswerDTO.DateCompleted;
            surveyResponseBO.IsNewRecord = surveyAnswerDTO.IsNewRecord;

            surveyResponseBO.UserId = userId.HasValue ? userId.Value : surveyAnswerDTO.UserId;
            surveyResponseBO.LastActiveUserId = surveyAnswerDTO.LastActiveUserId;

            surveyResponseBO.UserOrgId = surveyAnswerDTO.UserOrgId;
            surveyResponseBO.CurrentOrgId = surveyAnswerDTO.UserOrgId;
            surveyResponseBO.LastActiveOrgId = surveyAnswerDTO.LastActiveOrgId;

            surveyResponseBO.ResponseDetail.PageIds.AddRange(surveyAnswerDTO.ResponseDetail.PageResponseDetailList.Select(p => p.PageId).ToArray());
            surveyResponseBO.ResponseDetail.PageIds = surveyResponseBO.ResponseDetail.PageIds.Distinct().OrderBy(pid => pid).ToList();

            return surveyResponseBO;
        }

        public static ResponseContext ToResponseContext(this SurveyAnswerDTO surveyAnswerDTO)
        {
            MetadataAccessor metadataAccessor = new MetadataAccessor();
            var responseContext = new ResponseContext
            {
                ResponseId = surveyAnswerDTO.ResponseId,
                ParentResponseId = surveyAnswerDTO.ParentResponseId,
                RootResponseId = surveyAnswerDTO.RootResponseId,
                FormId = surveyAnswerDTO.SurveyId,
                FormName = metadataAccessor.GetFormName(surveyAnswerDTO.SurveyId),
                ParentFormId = metadataAccessor.GetParentFormId(surveyAnswerDTO.SurveyId),
                ParentFormName = metadataAccessor.GetParentFormName(surveyAnswerDTO.SurveyId),
                RootFormId = metadataAccessor.GetRootFormId(surveyAnswerDTO.SurveyId),
                RootFormName = metadataAccessor.GetRootFormName(surveyAnswerDTO.SurveyId),

                IsNewRecord = surveyAnswerDTO.IsNewRecord,

                UserOrgId = surveyAnswerDTO.LoggedInUserOrgId,
                UserId = surveyAnswerDTO.LoggedInUserId,
                UserName = surveyAnswerDTO.UserName
            }.ResolveMetadataDependencies() as ResponseContext;

            //if (responseContext.FormId != metadataAccessor.GetFormIdByViewId(surveyAnswerDTO.ViewId))
            //    throw new ArgumentException("ViewId not in agreement with FormId", string.Format("FormId={0}, ViewId=>{1}",
            //        responseContext.FormId, metadataAccessor.GetFormIdByViewId(surveyAnswerDTO.ViewId)));

            return responseContext;
        }
    }
}
