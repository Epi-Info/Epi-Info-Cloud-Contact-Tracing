using System.Collections.Generic;
using System.Linq;
using Epi.Web.EF;
using Epi.Web.Enter.Common.BusinessObject;

namespace Epi.Cloud.SurveyInfoServices.Extensions
{
    public static class SurveyMetadataExtensions
    {
        public static SurveyInfoBO ToSurveyInfoBO(this SurveyMetaData surveyMetadata, SurveyInfoBO surveyInfoBO)
        {
            surveyInfoBO.SurveyId = surveyMetadata.SurveyId.ToString();
            surveyInfoBO.SurveyName = surveyMetadata.SurveyName;
            surveyInfoBO.SurveyNumber = surveyMetadata.SurveyNumber;
            surveyInfoBO.IntroductionText = surveyMetadata.IntroductionText;
            surveyInfoBO.ExitText = surveyMetadata.ExitText;
            surveyInfoBO.OrganizationName = surveyMetadata.OrganizationName;
            surveyInfoBO.DepartmentName = surveyMetadata.DepartmentName;
            surveyInfoBO.ClosingDate = surveyMetadata.ClosingDate;
            surveyInfoBO.DateCreated = surveyMetadata.DateCreated;
            surveyInfoBO.IsDraftMode = surveyMetadata.IsDraftMode;
            surveyInfoBO.StartDate = surveyMetadata.StartDate;
            surveyInfoBO.IsSqlProject = (bool)surveyMetadata.IsSQLProject;
            surveyInfoBO.OwnerId = surveyMetadata.OwnerId;
            if (surveyMetadata.UserPublishKey != null)
            {
                // result.UserPublishKey = (Guid)entity.UserPublishKey.Value;
                surveyMetadata.UserPublishKey = surveyMetadata.UserPublishKey;
            }
            surveyInfoBO.SurveyType = surveyMetadata.SurveyTypeId;
            surveyInfoBO.ParentId = surveyMetadata.ParentId.ToString(); ;
            if (surveyMetadata.ViewId != null)
            {
                surveyInfoBO.ViewId = (int)surveyMetadata.ViewId;
            }
            //surveyInfoBO. = (bool)entity.ShowAllRecords;
            surveyInfoBO.IsShareable = (bool)surveyMetadata.IsShareable;

            return surveyInfoBO;
        }

        public static SurveyInfoBO ToSurveyInfoBO(this SurveyMetaData surveyMetadata)
        {
            var surveyInfoBO = new SurveyInfoBO();
            return surveyMetadata.ToSurveyInfoBO();
        }
        public static List<SurveyInfoBO> ToSurveyInfoBOList(this IEnumerable<SurveyMetaData> surveyMetadatas)
        {
            List<SurveyInfoBO> surveyInfoBOList = surveyMetadatas.Select(surveyMetadata => surveyMetadata.ToSurveyInfoBO()).ToList();
            return surveyInfoBOList;
        }

        public static FormInfoBO ToFormInfoBO(this SurveyMetaData surveyMetadata, FormInfoBO formInfoBO)
        {
            formInfoBO.FormId = surveyMetadata.SurveyId.ToString();
            formInfoBO.FormNumber = surveyMetadata.SurveyNumber;
            formInfoBO.FormName = surveyMetadata.SurveyName;
            formInfoBO.OrganizationName = surveyMetadata.OrganizationName;
            formInfoBO.OrganizationId = surveyMetadata.OrganizationId;
            formInfoBO.IsDraftMode = surveyMetadata.IsDraftMode;
            formInfoBO.UserId = surveyMetadata.OwnerId;
            formInfoBO.ParentId = (surveyMetadata.ParentId != null) ? surveyMetadata.ParentId.ToString() : "";

            return formInfoBO;
        }

        public static FormInfoBO ToFormInfoBO(this SurveyMetaData surveyMetadata)
        {
            FormInfoBO formInfoBO = new FormInfoBO();
            return surveyMetadata.ToFormInfoBO(formInfoBO);
        }

        public static FormInfoBO MapToFormInfoBO(this SurveyMetaData surveyMetadata, User UserEntity, bool includeMetadata = false)
        {
            FormInfoBO formInfoBO = new FormInfoBO();
            formInfoBO.IsSQLProject = surveyMetadata.IsSQLProject.HasValue ? surveyMetadata.IsSQLProject.Value : false;
            formInfoBO.FormId = surveyMetadata.SurveyId.ToString();
            formInfoBO.FormName = surveyMetadata.SurveyName;
            formInfoBO.FormNumber = surveyMetadata.SurveyNumber;
            formInfoBO.OrganizationName = surveyMetadata.OrganizationName;
            formInfoBO.OrganizationId = surveyMetadata.OrganizationId;
            formInfoBO.IsDraftMode = surveyMetadata.IsDraftMode;
            formInfoBO.UserId = surveyMetadata.OwnerId;

            formInfoBO.IsShareable = surveyMetadata.IsShareable.HasValue ? surveyMetadata.IsShareable.Value : false;

            formInfoBO.OwnerFName = UserEntity.FirstName;
            formInfoBO.OwnerLName = UserEntity.LastName;

            formInfoBO.ParentId = surveyMetadata.ParentId.ToString();

            if (includeMetadata)
            {
                formInfoBO.Xml = surveyMetadata.TemplateXML;
            }
            return formInfoBO;
        }
    }
}

