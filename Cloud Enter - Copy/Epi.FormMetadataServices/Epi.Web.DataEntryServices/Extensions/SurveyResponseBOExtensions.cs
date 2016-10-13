﻿using System;
using System.Linq;
using System.Collections.Generic;
using Epi.Web.Enter.Common.BusinessObject;
using Epi.Web.Enter.Common.DTO;
using Epi.DataPersistence.DataStructures;

namespace Epi.Cloud.DataEntryServices.Extensions
{
    public static class SurveyResponseBOExtensions
    {
        public static SurveyAnswerDTO ToSurveyAnswerDTO(this SurveyResponseBO surveyResponseBO)
        {
            SurveyAnswerDTO surveyAnswerDTO = new SurveyAnswerDTO();
            surveyAnswerDTO.ResponseId = surveyResponseBO.ResponseId;
            surveyAnswerDTO.SurveyId = surveyResponseBO.SurveyId;
            surveyAnswerDTO.DateUpdated = surveyResponseBO.DateUpdated;
            surveyAnswerDTO.DateCompleted = surveyResponseBO.DateCompleted;
            surveyAnswerDTO.DateCreated = surveyResponseBO.DateCreated;
            surveyAnswerDTO.Status = surveyResponseBO.Status;
			surveyAnswerDTO.ReasonForStatusChange = surveyResponseBO.ReasonForStatusChange;
			surveyAnswerDTO.UserPublishKey = surveyResponseBO.UserPublishKey;
            surveyAnswerDTO.IsDraftMode = surveyResponseBO.IsDraftMode;
            surveyAnswerDTO.IsLocked = surveyResponseBO.IsLocked;
            surveyAnswerDTO.ParentRecordId = surveyResponseBO.ParentRecordId;
            surveyAnswerDTO.UserEmail = surveyResponseBO.UserEmail;
            surveyAnswerDTO.LastActiveUserId = surveyResponseBO.LastActiveUserId;
            surveyAnswerDTO.RelateParentId = surveyResponseBO.RelateParentId;
            surveyAnswerDTO.RecordSourceId = surveyResponseBO.RecordSourceId;
            surveyAnswerDTO.ViewId = surveyResponseBO.ViewId;
            surveyAnswerDTO.FormOwnerId = 0; // TODO: Add FormOwnerId
            surveyAnswerDTO.LoggedInUserId = 0; // TODO: Do we need to populate loggedInUserId
            surveyAnswerDTO.RecoverLastRecordVersion = false; // TODO: Do we have to populate RecoverLastRecordVersion
            surveyAnswerDTO.RequestedViewId = string.Empty;
            surveyAnswerDTO.CurrentPageNumber = 0;

            surveyAnswerDTO.ResponseDetail = surveyResponseBO.ResponseDetail;

            return surveyAnswerDTO;
        }

        public static List<SurveyAnswerDTO> ToSurveyAnswerDTOList(this IEnumerable<SurveyResponseBO> surveyResponseBOList)
        {
            return surveyResponseBOList.Select(bo => bo.ToSurveyAnswerDTO()).ToList();
        }

        public static SurveyResponse ToSurveyResponse(this SurveyResponseBO surveyResponseBO, int orgId = -1)
        {
            var surveyResponse = new SurveyResponse();
            Guid relateParentId = Guid.Empty;
            if (!string.IsNullOrEmpty(surveyResponseBO.RelateParentId))
            {
                relateParentId = new Guid(surveyResponseBO.RelateParentId);
            }
            Guid parentRecordId = Guid.Empty;
            if (!string.IsNullOrEmpty(surveyResponseBO.ParentRecordId))
            {
                parentRecordId = new Guid(surveyResponseBO.ParentRecordId);
            }
            surveyResponse.SurveyId = new Guid(surveyResponseBO.SurveyId);
            surveyResponse.ResponseId = new Guid(surveyResponseBO.ResponseId);
            surveyResponse.StatusId = surveyResponseBO.Status;
            surveyResponse.DateUpdated = surveyResponseBO.DateUpdated;
            surveyResponse.DateCompleted = surveyResponseBO.DateCompleted;
            surveyResponse.DateCreated = surveyResponseBO.DateCreated;
            surveyResponse.IsDraftMode = surveyResponseBO.IsDraftMode;
            surveyResponse.RecordSourceId = surveyResponseBO.RecordSourceId;
            surveyResponse.ResponseDetail = surveyResponseBO.ResponseDetail;
            if (!string.IsNullOrEmpty(surveyResponseBO.RelateParentId) && relateParentId != Guid.Empty)
            {
                surveyResponse.RelateParentId = new Guid(surveyResponseBO.RelateParentId);
            }
            if (!string.IsNullOrEmpty(surveyResponseBO.ParentRecordId) && parentRecordId != Guid.Empty)
            {
                surveyResponse.ParentRecordId = new Guid(surveyResponseBO.ParentRecordId);
            }
            if (orgId != -1)
            {
                surveyResponse.OrganizationId = orgId;
            }
            return surveyResponse;
        }

        public static SurveyResponseBO MergeIntoSurveyResponseBO(this SurveyResponseBO surveyResponseBO, SurveyInfoBO parentSurveyInfoBO, string relateParentId)
        {
            surveyResponseBO.ParentId = parentSurveyInfoBO.ParentId;
            surveyResponseBO.RelateParentId = relateParentId;
            surveyResponseBO.IsDraftMode = parentSurveyInfoBO.IsDraftMode;

            var responseDetail = surveyResponseBO.ResponseDetail;
            responseDetail.ParentFormId = parentSurveyInfoBO.ParentId;
            responseDetail.RelateParentResponseId = relateParentId;
            responseDetail.IsRelatedView = surveyResponseBO.ParentId != null;
            responseDetail.IsDraftMode = parentSurveyInfoBO.IsDraftMode;
            responseDetail.RecStatus = surveyResponseBO.Status;
            responseDetail.GlobalRecordID = surveyResponseBO.ResponseId;

            return surveyResponseBO;
        }
    }
}
