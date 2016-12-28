using System;
using Epi.Cloud.Common.Constants;
using Epi.FormMetadata.DataStructures;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.DTO;
using Epi.Web.MVC.Models;

namespace Epi.Cloud.MVC.Extensions
{
    /// <summary>
    /// Maps DTO object to Model object or Model object to DTO object
    /// </summary>
    public static class FormInfoDTOExtensions
    {
        /// <summary>
        /// Maps FormInfo DTO to FormInfo Model
        /// </summary>
        /// <param name="FormInfoDTO"></param>
        /// <returns></returns>
        public static FormInfoModel ToFormInfoModel(this FormInfoDTO FormInfoDTO)
        {
            return new FormInfoModel
            {
                IsSQLProject = FormInfoDTO.IsSQLProject,
                FormId = FormInfoDTO.FormId,
                FormName = FormInfoDTO.FormName,
                FormNumber = FormInfoDTO.FormNumber,
                IsDraftMode = FormInfoDTO.IsDraftMode,
                OrganizationId = FormInfoDTO.OrganizationId,
                OrganizationName = FormInfoDTO.OrganizationName,
                UserId = FormInfoDTO.UserId,
                IsOwner = FormInfoDTO.IsOwner,
                OwnerFName = FormInfoDTO.OwnerFName,
                OwnerLName = FormInfoDTO.OwnerLName,
                IsShareable = FormInfoDTO.IsShareable,
                IsShared = FormInfoDTO.IsShared,
                DataAccessRuleId = FormInfoDTO.DataAccessRuleId,
            };
        }

        public static SurveyInfoBO ToSurveyInfoBO(this FormInfoDTO formInfoDTO, int viewId)
        {
            return new SurveyInfoBO
            {
                DataAccessRuleId = formInfoDTO.DataAccessRuleId,
                HasDraftModeData = formInfoDTO.HasDraftModeData,
                IsDraftMode = formInfoDTO.IsDraftMode,
                IsShareable = formInfoDTO.IsShareable,
                IsShared = formInfoDTO.IsShared,
                OrganizationName = formInfoDTO.OrganizationName,
                OwnerId = formInfoDTO.OwnerId,
                SurveyId = formInfoDTO.FormId,
                SurveyName = formInfoDTO.FormName,
                SurveyNumber = formInfoDTO.FormName,

                ViewId = viewId,

                DateCreated = DateTime.MinValue,
                StartDate = DateTime.MinValue,
                ClosingDate = DateTime.MinValue,

                SurveyType = SurveyTypes.MultipleResponse,
                OrganizationKey = Guid.Empty,
                UserPublishKey = Guid.Empty,
                DepartmentName = string.Empty,
                IntroductionText = string.Empty,
                ExitText = string.Empty,

                IsSqlProject = formInfoDTO.IsSQLProject,
                DBConnectionString = null,

                ParentId = null
            };
        }

        public static SurveyInfoDTO ToSurveyInfoDTO(this FormInfoDTO formInfoDTO, FormDigest formDigest)
        {
            return new SurveyInfoDTO
            {
                DataAccessRuleId = formInfoDTO.DataAccessRuleId,
                HasDraftModeData = formInfoDTO.HasDraftModeData,
                IsDraftMode = formInfoDTO.IsDraftMode,
                IsShareable = formInfoDTO.IsShareable,
                IsShared = formInfoDTO.IsShared,
                OrganizationName = formInfoDTO.OrganizationName,
                OwnerId = formInfoDTO.OwnerId,
                SurveyId = formInfoDTO.FormId,
                SurveyName = formInfoDTO.FormName,
                SurveyNumber = formInfoDTO.FormName,

                ViewId = formDigest.ViewId,

                StartDate = DateTime.MinValue,
                ClosingDate = DateTime.MinValue,

                SurveyType = SurveyTypes.MultipleResponse,
                OrganizationKey = Guid.Empty,
                UserPublishKey = Guid.Empty,
                DepartmentName = string.Empty,
                IntroductionText = string.Empty,
                ExitText = string.Empty,

                IsSqlProject = formInfoDTO.IsSQLProject,
                DBConnectionString = null,

                ParentId = null
            };
        }
    }
}