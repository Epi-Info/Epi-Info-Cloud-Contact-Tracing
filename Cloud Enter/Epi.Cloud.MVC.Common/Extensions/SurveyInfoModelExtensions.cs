using Epi.Cloud.Common.DTO;
using Epi.Cloud.Common.Model;

namespace Epi.Cloud.MVC.Extensions
{
    public static class SurveyInfoModelExtensions
    {
        /// <summary>
        /// Maps SurveyInfo Model to SurveyInfo DTO.
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public static SurveyInfoDTO ToSurveyInfoDTO(this SurveyInfoModel surveyInfoModel)
        {
            return new SurveyInfoDTO
            {
                SurveyId = surveyInfoModel.SurveyId,
                SurveyNumber = surveyInfoModel.SurveyNumber,
                SurveyName = surveyInfoModel.SurveyName,
                OrganizationName = surveyInfoModel.OrganizationName,
                DepartmentName = surveyInfoModel.DepartmentName,
                IntroductionText = surveyInfoModel.IntroductionText,
                ExitText = surveyInfoModel.ExitText,
                IsSuccess = surveyInfoModel.IsSuccess,
                ClosingDate = surveyInfoModel.ClosingDate,
                UserPublishKey = surveyInfoModel.UserPublishKey,
                IsDraftMode = surveyInfoModel.IsDraftMode,
                StartDate = surveyInfoModel.StartDate,
            };
        }
    }
}
