
using System.Collections.Generic;
using Epi.Cloud.Common.DTO;
using Epi.Web.MVC.Models;

namespace Epi.Cloud.MVC.Extensions
{
    public static class SurveyAnswerDTOExtensions
    {
        public static SurveyAnswerModel ToSurveyAnswerModel(this SurveyAnswerDTO surveyAnswerDTO)
        {
            return new SurveyAnswerModel
            {
                ResponseId = surveyAnswerDTO.ResponseId,
                SurveyId = surveyAnswerDTO.SurveyId,
                DateUpdated = surveyAnswerDTO.DateUpdated,
                DateCompleted = surveyAnswerDTO.DateCompleted,
                Status = surveyAnswerDTO.Status,

                ResponseDetail = surveyAnswerDTO.ResponseDetail,
            };
        }

        public static List<SurveyAnswerModel> ToSurveyAnswerModel(this List<SurveyAnswerDTO> surveyAnswerDTOList)
        {
            List<SurveyAnswerModel> ModelList = new List<SurveyAnswerModel>();
            foreach (var surveyAnswerDTO in surveyAnswerDTOList)
            {
                SurveyAnswerModel surveyAnswerModel = new SurveyAnswerModel();
                surveyAnswerModel.ResponseId = surveyAnswerDTO.ResponseId;
                surveyAnswerModel.SurveyId = surveyAnswerDTO.SurveyId;
                surveyAnswerModel.DateUpdated = surveyAnswerDTO.DateUpdated;
                surveyAnswerModel.DateCompleted = surveyAnswerDTO.DateCompleted;
                surveyAnswerModel.Status = surveyAnswerDTO.Status;
                surveyAnswerModel.ResponseDetail = surveyAnswerDTO.ResponseDetail;
                surveyAnswerModel.ParentRecordId = surveyAnswerDTO.ParentRecordId;
                surveyAnswerModel.RelateParentId = surveyAnswerDTO.RelateParentId;
                ModelList.Add(surveyAnswerModel);
            }
            return ModelList;
        }
    }
}
