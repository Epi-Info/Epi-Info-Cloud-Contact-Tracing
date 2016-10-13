using System;
using Epi.Web.EF;
using Epi.Web.Enter.Common.BusinessObject;

namespace Epi.Cloud.DataEntryServices.Extensions
{
    public static class SurveyInfoBOExtensions
    {
        public static SurveyMetaData ToSurveyMetadata(this SurveyInfoBO surveyInfoBO)
        {
            SurveyMetaData surveyMetaData = new SurveyMetaData();

            surveyMetaData.SurveyId = new Guid(surveyInfoBO.SurveyId);
            surveyMetaData.SurveyName = surveyInfoBO.SurveyName;
            surveyMetaData.SurveyNumber = surveyInfoBO.SurveyNumber;
            surveyMetaData.IntroductionText = surveyInfoBO.IntroductionText;
            surveyMetaData.ExitText = surveyInfoBO.ExitText;
            surveyMetaData.OrganizationName = surveyInfoBO.OrganizationName;
            surveyMetaData.DepartmentName = surveyInfoBO.DepartmentName;
            surveyMetaData.ClosingDate = surveyInfoBO.ClosingDate;
            surveyMetaData.UserPublishKey = surveyInfoBO.UserPublishKey;
            surveyMetaData.SurveyTypeId = surveyInfoBO.SurveyType;
            surveyMetaData.DateCreated = surveyInfoBO.DateCreated;
            surveyMetaData.IsDraftMode = surveyInfoBO.IsDraftMode;
            surveyMetaData.StartDate = surveyInfoBO.StartDate;
            surveyMetaData.OwnerId = surveyInfoBO.OwnerId;
            surveyMetaData.ViewId = surveyInfoBO.ViewId;
            surveyMetaData.IsSQLProject = surveyInfoBO.IsSqlProject;
            surveyMetaData.IsShareable = surveyInfoBO.IsShareable;
            surveyMetaData.DataAccessRuleId = surveyInfoBO.DataAccessRuleId;
            if (!string.IsNullOrEmpty(surveyInfoBO.ParentId))
            {
                surveyMetaData.ParentId = new Guid(surveyInfoBO.ParentId);
            }

            return surveyMetaData;
        }

        public static SurveyMetaData ToEF(SurveyInfoBO surveyInfoBO)
        {
            SurveyMetaData surveyMetadata = new SurveyMetaData();
            surveyMetadata.SurveyName = surveyInfoBO.SurveyName;
            surveyMetadata.SurveyNumber = surveyInfoBO.SurveyNumber;
            surveyMetadata.IntroductionText = surveyInfoBO.IntroductionText;
            surveyMetadata.ExitText = surveyInfoBO.ExitText;
            surveyMetadata.OrganizationName = surveyInfoBO.OrganizationName;
            surveyMetadata.DepartmentName = surveyInfoBO.DepartmentName;
            surveyMetadata.ClosingDate = surveyInfoBO.ClosingDate;
            surveyMetadata.SurveyTypeId = surveyInfoBO.SurveyType;
            surveyMetadata.UserPublishKey = surveyInfoBO.UserPublishKey;
            surveyMetadata.IsDraftMode = surveyInfoBO.IsDraftMode;
            surveyMetadata.StartDate = surveyInfoBO.StartDate;
            return surveyMetadata;
        }
    }
}
