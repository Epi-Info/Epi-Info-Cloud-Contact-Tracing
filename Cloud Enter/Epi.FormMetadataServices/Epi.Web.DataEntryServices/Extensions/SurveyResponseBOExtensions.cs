using System;
using System.Linq;
using System.Collections.Generic;
using Epi.Web.Enter.Common.BusinessObject;
using Epi.Web.Enter.Common.DTO;

namespace Epi.Cloud.DataEntryServices.Extensions
{
    public static class SurveyResponseBOExtensions
    {
        public static SurveyAnswerDTO ToSurveyAnswerDTO(this SurveyResponseBO businessobject)
        {
            SurveyAnswerDTO surveyAnswerDTO = new SurveyAnswerDTO();
            surveyAnswerDTO.SurveyId = businessobject.SurveyId;
            surveyAnswerDTO.ResponseId = businessobject.ResponseId;
            surveyAnswerDTO.XML = businessobject.XML;
            surveyAnswerDTO.Status = businessobject.Status;
            surveyAnswerDTO.UserPublishKey = businessobject.UserPublishKey;
            surveyAnswerDTO.DateUpdated = businessobject.DateUpdated;
            surveyAnswerDTO.DateCompleted = businessobject.DateCompleted;
            surveyAnswerDTO.ResponseDetail = businessobject.ResponseDetail;

            return surveyAnswerDTO;
        }

        public static List<SurveyAnswerDTO> ToSurveyAnswerDTOList(this IEnumerable<SurveyResponseBO> surveyResponseBOList)
        {
            return surveyResponseBOList.Select(bo => bo.ToSurveyAnswerDTO()).ToList();
        }

        public static Epi.Web.EF.SurveyResponse ToSurveyResponse(this SurveyResponseBO surveyResponseBO, int orgId = -1)
        {
            var surveyResponse = new Epi.Web.EF.SurveyResponse();
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
            surveyResponse.ResponseXML = surveyResponseBO.XML;
            surveyResponse.ResponseXMLSize = surveyResponseBO.TemplateXMLSize;
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
    }
}
