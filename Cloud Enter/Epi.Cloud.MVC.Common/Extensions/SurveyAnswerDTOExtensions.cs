
using System.Collections.Generic;
using Epi.Cloud.Common.DTO;
using Epi.Common.Core.Interfaces;
using Epi.Cloud.MVC.Models;

namespace Epi.Cloud.MVC.Extensions
{
    public static class SurveyAnswerDTOExtensions
    {
        public static SurveyAnswerModel ToSurveyAnswerModel(this SurveyAnswerDTO surveyAnswerDTO, string responseId)
        {
            SurveyAnswerModel surveyAnswerModel = ((IResponseContext)surveyAnswerDTO).ToSurveyAnswerModel();

            if (responseId == null || responseId == surveyAnswerDTO.ResponseDetail.ResponseId)
            {
                surveyAnswerModel.SurveyId = surveyAnswerDTO.SurveyId;
                surveyAnswerModel.DateUpdated = surveyAnswerDTO.DateUpdated;
                surveyAnswerModel.DateCompleted = surveyAnswerDTO.DateCompleted;
                surveyAnswerModel.Status = surveyAnswerDTO.Status;
            }
            else
            {
                var childResponseDetail = surveyAnswerDTO.ResponseDetail.FindFormResponseDetail(responseId);
                surveyAnswerModel.SurveyId = childResponseDetail.FormId;
                surveyAnswerModel.DateUpdated = childResponseDetail.LastSaveTime;
                surveyAnswerModel.DateCompleted = childResponseDetail.LastSaveTime;
                surveyAnswerModel.Status = childResponseDetail.RecStatus;
            }

            return surveyAnswerModel;
        }

        public static List<SurveyAnswerModel> ToSurveyAnswerModel(this List<SurveyAnswerDTO> surveyAnswerDTOList)
        {
            List<SurveyAnswerModel> surveyAnswerModelList = new List<SurveyAnswerModel>();
            foreach (var surveyAnswerDTO in surveyAnswerDTOList)
            {
                var surveyAnswerModel = surveyAnswerDTO.ToSurveyAnswerModel(surveyAnswerDTO.ResponseId);
                surveyAnswerModelList.Add(surveyAnswerModel);
            }
            return surveyAnswerModelList;
        }
    }
}
