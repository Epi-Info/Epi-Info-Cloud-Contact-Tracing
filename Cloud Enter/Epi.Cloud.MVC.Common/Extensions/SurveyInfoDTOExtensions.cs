using System.Collections.Generic;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.DTO;
using Epi.Cloud.Common.Model;

namespace Epi.Cloud.MVC.Extensions
{
    public static class SurveyInfoDTOExtensions
    {
        public static SurveyInfoBO ToSurveyInfoBO(this SurveyInfoDTO surveyInfoDTO)
        {
            return new SurveyInfoBO
            {
                SurveyId = surveyInfoDTO.SurveyId,
                SurveyName = surveyInfoDTO.SurveyName,
                SurveyNumber = surveyInfoDTO.SurveyNumber,
                IntroductionText = surveyInfoDTO.IntroductionText,
                ExitText = surveyInfoDTO.ExitText,
                OrganizationName = surveyInfoDTO.OrganizationName,
                DepartmentName = surveyInfoDTO.DepartmentName,
                ClosingDate = surveyInfoDTO.ClosingDate,
                UserPublishKey = surveyInfoDTO.UserPublishKey,
                SurveyType = surveyInfoDTO.SurveyType,
                OrganizationKey = surveyInfoDTO.OrganizationKey,
                IsDraftMode = surveyInfoDTO.IsDraftMode,
                StartDate = surveyInfoDTO.StartDate,
                OwnerId = surveyInfoDTO.OwnerId,
                DBConnectionString = surveyInfoDTO.DBConnectionString,
                IsSqlProject = surveyInfoDTO.IsSqlProject,
                ViewId = surveyInfoDTO.ViewId,
                IsShareable = surveyInfoDTO.IsShareable,
                DataAccessRuleId = surveyInfoDTO.DataAccessRuleId,
            };
        }

        public static List<SurveyInfoBO> ToSurveyInfoBOList(this List<SurveyInfoDTO> surveyInfoDTOList)
        {
            List<SurveyInfoBO> result = new List<SurveyInfoBO>();
            foreach (SurveyInfoDTO surveyInfoDTO in surveyInfoDTOList)
            {
                result.Add(surveyInfoDTO.ToSurveyInfoBO());
            };

            return result;
        }

        /// <summary>
        /// Maps SurveyInfo DTO to SurveyInfo Model.
        /// </summary>
        /// <param name="surveyInfoDTO"></param>
        /// <returns></returns>
        public static SurveyInfoModel ToSurveyInfoModel(this SurveyInfoDTO surveyInfoDTO)
        {
            return new SurveyInfoModel
            {
                SurveyId = surveyInfoDTO.SurveyId,
                SurveyNumber = surveyInfoDTO.SurveyNumber,
                SurveyName = surveyInfoDTO.SurveyName,
                OrganizationName = surveyInfoDTO.OrganizationName,
                DepartmentName = surveyInfoDTO.DepartmentName,
                IntroductionText = surveyInfoDTO.IntroductionText,
                ExitText = surveyInfoDTO.ExitText,
                IsSuccess = surveyInfoDTO.IsSuccess,
                SurveyType = surveyInfoDTO.SurveyType,
                ClosingDate = surveyInfoDTO.ClosingDate,
                UserPublishKey = surveyInfoDTO.UserPublishKey,
                IsDraftMode = surveyInfoDTO.IsDraftMode,
                StartDate = surveyInfoDTO.StartDate,
                IsSqlProject = surveyInfoDTO.IsSqlProject,
                FormOwnerId = surveyInfoDTO.OwnerId,
            };
        }

        public static SurveyInfoModel ToFormInfoModel(this SurveyInfoDTO SurveyInfoDTO)
        {
            return new SurveyInfoModel
            {
                SurveyId = SurveyInfoDTO.SurveyId,
                SurveyNumber = SurveyInfoDTO.SurveyNumber,
                SurveyName = SurveyInfoDTO.SurveyName,
                OrganizationName = SurveyInfoDTO.OrganizationName,
                DepartmentName = SurveyInfoDTO.DepartmentName,
                IntroductionText = SurveyInfoDTO.IntroductionText,
                ExitText = SurveyInfoDTO.ExitText,
                IsSuccess = SurveyInfoDTO.IsSuccess,
                SurveyType = SurveyInfoDTO.SurveyType,
                ClosingDate = SurveyInfoDTO.ClosingDate,
                UserPublishKey = SurveyInfoDTO.UserPublishKey,
                IsDraftMode = SurveyInfoDTO.IsDraftMode,
                StartDate = SurveyInfoDTO.StartDate,
                IsSqlProject = SurveyInfoDTO.IsSqlProject,
                FormOwnerId = SurveyInfoDTO.OwnerId,
            };
        }
    }
}
