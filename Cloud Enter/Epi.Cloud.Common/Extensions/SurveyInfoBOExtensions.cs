using System.Collections.Generic;
using System.Linq;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.DTO;
using Epi.Common.Core.DataStructures;

namespace Epi.Cloud.Common.Extensions
{
    public static class SurveyInfoBOExtensions
    {
        public static FormInfoDTO ToFormInfoDTO(this SurveyInfoBO surveyInfoBO)
        {
            return new FormInfoDTO
            {
                IsSQLProject = surveyInfoBO.IsSqlProject,
                FormId = surveyInfoBO.SurveyId,
                FormNumber = surveyInfoBO.SurveyNumber,
                FormName = surveyInfoBO.SurveyName,
                OrganizationName = surveyInfoBO.OrganizationName,
                OwnerId = surveyInfoBO.OwnerId,
                IsDraftMode = surveyInfoBO.IsDraftMode
            };
        }


        public static FormsHierarchyBO ToFormsHierarchyBO(this SurveyInfoBO surveyInfoBO, string rootId)
        {
            return new FormsHierarchyBO
            {
                RootFormId = rootId,
                FormId = surveyInfoBO.SurveyId,
                ViewId = surveyInfoBO.ViewId,
                SurveyInfo = surveyInfoBO,
                IsRoot = surveyInfoBO.SurveyId == rootId
            };
        }

        public static List<FormsHierarchyBO> ToFormsHierarchyBOList(this List<SurveyInfoBO> surveyInfoBOList, string rootId)
        {
            return surveyInfoBOList.Select(surveyInfoBO => surveyInfoBO.ToFormsHierarchyBO(rootId)).ToList();
        }

        public static SurveyInfoDTO ToSurveyInfoDTO(this SurveyInfoBO surveyInfoBO)
        {
            return new SurveyInfoDTO
            {
                ClosingDate = surveyInfoBO.ClosingDate,
                DataAccessRuleId = surveyInfoBO.DataAccessRuleId,
                DBConnectionString = surveyInfoBO.DBConnectionString,
                DepartmentName = surveyInfoBO.DepartmentName,
                ExitText = surveyInfoBO.ExitText,
                IntroductionText = surveyInfoBO.IntroductionText,
                IsDraftMode = surveyInfoBO.IsDraftMode,
                IsShareable = surveyInfoBO.IsShareable,
                IsShared = surveyInfoBO.IsShared,
                IsSqlProject = surveyInfoBO.IsSqlProject,
                OrganizationKey = surveyInfoBO.OrganizationKey,
                OrganizationName = surveyInfoBO.OrganizationName,
                OwnerId = surveyInfoBO.OwnerId,
                ParentFormId = surveyInfoBO.ParentFormId,
                StartDate = surveyInfoBO.StartDate,
                SurveyId = surveyInfoBO.SurveyId,
                SurveyName = surveyInfoBO.SurveyName,
                SurveyNumber = surveyInfoBO.SurveyNumber,
                SurveyType = surveyInfoBO.SurveyType,
                UserPublishKey = surveyInfoBO.UserPublishKey,
                ViewId = surveyInfoBO.ViewId,
                HasDraftModeData = surveyInfoBO.HasDraftModeData,
            };
        }

        public static List<SurveyInfoDTO> ToSurveyInfoDTOList(this List<SurveyInfoBO> surveyInfoBOList)
        {
            return surveyInfoBOList.Select(surveyInfoBO => surveyInfoBO.ToSurveyInfoDTO()).ToList();
        }

        //public static ResponseContext ToResponseContext(this SurveyInfoBO surveyInfoBO, ResponseContext mergeInto = null)
        //{
        //    var responseContext = mergeInto != null ? mergeInto : new ResponseContext();
        //    responseContext.FormId = surveyInfoBO.SurveyId;
        //    responseContext.ParentFormId = surveyInfoBO.ParentFormId;
        //    return responseContext.ResolveMetadataDependencies();
        //}
    }
}
