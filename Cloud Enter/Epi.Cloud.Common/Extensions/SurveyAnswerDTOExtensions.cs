using System.Collections.Generic;
using System.Linq;
using Epi.DataPersistence.Constants;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.DTO;

namespace Epi.Cloud.Common.Extensions
{
    public static class SurveyAnswerDTOExtensions
    {
        public static SurveyResponseBO ToSurveyResponseBO(this SurveyAnswerDTO surveyAnswerDTO, int? userId = null)
        {
            SurveyResponseBO surveyResponseBO = new SurveyResponseBO();
            surveyResponseBO.SurveyId = surveyAnswerDTO.SurveyId;
            surveyResponseBO.ResponseId = surveyAnswerDTO.ResponseId;
            surveyResponseBO.Status = surveyAnswerDTO.Status;
            surveyResponseBO.ReasonForStatusChange = surveyAnswerDTO.ReasonForStatusChange;
            surveyResponseBO.UserPublishKey = surveyAnswerDTO.UserPublishKey;
            surveyResponseBO.DateUpdated = surveyAnswerDTO.DateUpdated;
            surveyResponseBO.DateCompleted = surveyAnswerDTO.DateCompleted;
            surveyResponseBO.ResponseDetail = surveyAnswerDTO.ResponseDetail;

            surveyResponseBO.ResponseDetail.PageIds.AddRange(surveyAnswerDTO.ResponseDetail.PageResponseDetailList.Select(p => p.PageId).ToArray());
            surveyResponseBO.ResponseDetail.PageIds = surveyResponseBO.ResponseDetail.PageIds.Distinct().OrderBy(pid => pid).ToList();

            if (userId.HasValue) surveyResponseBO.UserId = userId.Value;
            return surveyResponseBO;
        }

        public static List<SurveyResponseBO> ToSurveyResponseBOList(this IEnumerable<SurveyAnswerDTO> surveyAnswerDTOList, int? userId = null)
        {
            List<SurveyResponseBO> result = surveyAnswerDTOList.Select(dto => dto.ToSurveyResponseBO(userId)).ToList();
            return result;
        }
    }
}
