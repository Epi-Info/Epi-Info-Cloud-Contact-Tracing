
using System.Collections.Generic;
using Epi.Cloud.Common.DTO;
using Epi.Common.Core.Interfaces;
using Epi.Web.MVC.Models;

namespace Epi.Cloud.MVC.Extensions
{
    public static class SurveyAnswerDTOExtensions
    {
        public static SurveyAnswerModel ToSurveyAnswerModel(this SurveyAnswerDTO surveyAnswerDTO)
        {
            SurveyAnswerModel surveyAnswerModel = ((IResponseContext)surveyAnswerDTO).ToSurveyAnswerModel();

            surveyAnswerModel.SurveyId = surveyAnswerDTO.SurveyId;
            surveyAnswerModel.DateUpdated = surveyAnswerDTO.DateUpdated;
            surveyAnswerModel.DateCompleted = surveyAnswerDTO.DateCompleted;
            surveyAnswerModel.Status = surveyAnswerDTO.Status;

            return surveyAnswerModel;

        }

        public static List<SurveyAnswerModel> ToSurveyAnswerModel(this List<SurveyAnswerDTO> surveyAnswerDTOList)
        {
            List<SurveyAnswerModel> surveyAnswerModelList = new List<SurveyAnswerModel>();
            foreach (var surveyAnswerDTO in surveyAnswerDTOList)
            {
                var surveyAnswerModel = surveyAnswerDTO.ToSurveyAnswerModel();
                surveyAnswerModelList.Add(surveyAnswerModel);
            }
            return surveyAnswerModelList;
        }
    }
}
