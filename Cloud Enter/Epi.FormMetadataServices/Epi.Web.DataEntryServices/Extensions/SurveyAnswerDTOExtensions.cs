using System;
using System.Linq;
using System.Collections.Generic;
using Epi.Web.Enter.Common.BusinessObject;
using Epi.Web.Enter.Common.DTO;

namespace Epi.Cloud.DataEntryServices.Extensions
{
    public static class SurveyAnswerDTOExtensions
    {
        public static SurveyResponseBO ToSurveyResponseBO(this SurveyAnswerDTO surveyAnswerDTO)
        {
            SurveyResponseBO surveyResponseBO = new SurveyResponseBO();
            surveyResponseBO.SurveyId = surveyAnswerDTO.SurveyId;
            surveyResponseBO.ResponseId = surveyAnswerDTO.ResponseId;
            surveyResponseBO.XML = surveyAnswerDTO.XML;
            surveyResponseBO.Status = surveyAnswerDTO.Status;
            surveyResponseBO.UserPublishKey = surveyAnswerDTO.UserPublishKey;
            surveyResponseBO.DateUpdated = surveyAnswerDTO.DateUpdated;
            surveyResponseBO.DateCompleted = surveyAnswerDTO.DateCompleted;
            surveyResponseBO.ResponseDetail = surveyAnswerDTO.ResponseDetail;
            return surveyResponseBO;
        }

        public static List<SurveyResponseBO> ToSurveyResponseBOList(this IEnumerable<SurveyAnswerDTO> surveyAnswerDTOList)
        {
            List<SurveyResponseBO> result = surveyAnswerDTOList.Select(dto => dto.ToSurveyResponseBO()).ToList();
            return result;
        }
    }
}
