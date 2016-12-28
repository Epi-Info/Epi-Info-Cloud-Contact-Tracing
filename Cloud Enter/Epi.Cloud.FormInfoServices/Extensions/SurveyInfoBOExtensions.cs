using System;
using System.Collections.Generic;
using System.Linq;
using Epi.Web.EF;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.DTO;

namespace Epi.Cloud.SurveyInfoServices.Extensions
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

        public static SurveyMetaData ToEF(this SurveyInfoBO surveyInfoBO)
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
				ParentId = surveyInfoBO.ParentId,
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

		public static List<SurveyInfoDTO> ToSurveyInfoDTOList(this IEnumerable<SurveyInfoBO> surveyInfoBOs)
		{
			return surveyInfoBOs.Select(bo => bo.ToSurveyInfoDTO()).ToList();
		}

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
    }
}
