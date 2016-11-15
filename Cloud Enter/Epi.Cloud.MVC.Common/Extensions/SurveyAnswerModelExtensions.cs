using Epi.Web.Enter.Common.DTO;
using Epi.Web.MVC.Models;

namespace Epi.Cloud.MVC.Extensions
{
    public static class SurveyAnswerModelExtensions
    {
        /// <summary>
        /// Maps SurveyInfo Model to SurveyInfo DTO.
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public static SurveyAnswerDTO ToSurveyAnswerDTO(this SurveyAnswerModel surveyAnswerModel)
        {
            return new SurveyAnswerDTO
            {
                ResponseId = surveyAnswerModel.ResponseId,
                SurveyId = surveyAnswerModel.SurveyId,
                DateUpdated = surveyAnswerModel.DateUpdated,
                DateCompleted = surveyAnswerModel.DateCompleted,
                Status = surveyAnswerModel.Status,
                RelateParentId = surveyAnswerModel.RelateParentId,
            };
        }
    }
}
