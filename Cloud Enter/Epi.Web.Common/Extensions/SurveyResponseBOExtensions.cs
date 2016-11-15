using System;
using System.Linq;
using System.Collections.Generic;
using Epi.DataPersistence.DataStructures;
using Epi.Web.Enter.Common.BusinessObject;
using Epi.Web.Enter.Common.DTO;

namespace Epi.Web.Enter.Common.Extensions
{
    public static class SurveyResponseBOExtensions
    {
        public static SurveyAnswerDTO ToSurveyAnswerDTO(this SurveyResponseBO surveyResponseBO)
        {
            return new SurveyAnswerDTO
            {
                ResponseId = surveyResponseBO.ResponseId,
                SurveyId = surveyResponseBO.SurveyId,
                DateUpdated = surveyResponseBO.DateUpdated,
                DateCompleted = surveyResponseBO.DateCompleted,
                DateCreated = surveyResponseBO.DateCreated,
                Status = surveyResponseBO.Status,
                ReasonForStatusChange = surveyResponseBO.ReasonForStatusChange,
                UserPublishKey = surveyResponseBO.UserPublishKey,
                IsDraftMode = surveyResponseBO.IsDraftMode,
                IsLocked = surveyResponseBO.IsLocked,
                ParentRecordId = surveyResponseBO.ParentRecordId,
                UserEmail = surveyResponseBO.UserEmail,
                LastActiveUserId = surveyResponseBO.LastActiveUserId,
                RelateParentId = surveyResponseBO.RelateParentId,
                RecordSourceId = surveyResponseBO.RecordSourceId,
                ViewId = surveyResponseBO.ViewId,
                FormOwnerId = 0, // TODO: Add FormOwnerId
                LoggedInUserId = surveyResponseBO.UserId,
                RecoverLastRecordVersion = false, // TODO: Do we have to populate RecoverLastRecordVersion
                RequestedViewId = string.Empty,
                CurrentPageNumber = 0,
                ResponseDetail = surveyResponseBO.ResponseDetail
            };
        }
        public static List<SurveyAnswerDTO> ToSurveyAnswerDTOList(this List<SurveyResponseBO> surveyResponseBOList)
        {
            return surveyResponseBOList.Select(surveyResponseBO => surveyResponseBO.ToSurveyAnswerDTO()).ToList();
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
            responseDetail.IsNewRecord = surveyResponseBO.IsNewRecord;
            responseDetail.RecStatus = surveyResponseBO.Status;
            responseDetail.GlobalRecordID = surveyResponseBO.ResponseId;

            return surveyResponseBO;
        }
    }
}
