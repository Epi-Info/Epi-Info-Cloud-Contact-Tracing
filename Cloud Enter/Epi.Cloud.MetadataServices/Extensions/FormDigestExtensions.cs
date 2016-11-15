using System.Collections.Generic;
using System.Linq;
using Epi.FormMetadata.DataStructures;
using Epi.Web.Enter.Common.BusinessObject;
using Epi.Web.Enter.Common.DTO;

namespace Epi.Cloud.MetadataServices.Extensions
{
    public static class FormDigestExtensions
    {
        public static SurveyInfoBO ToSurveyInfoBO(this FormDigest formDigest)
        {
            var surveyInfoBO = new SurveyInfoBO();
            return formDigest.MergeIntoSurveyInfoBO(surveyInfoBO);
        }

        public static List<SurveyInfoBO> ToSurveyInfoBOList(this FormDigest[] formDigests)
        {
            List<SurveyInfoBO> surveyInfoBOs = formDigests.Select(d => d.ToSurveyInfoBO()).ToList();
            return surveyInfoBOs;
        }

        public static SurveyInfoBO MergeIntoSurveyInfoBO(this FormDigest formDigest, SurveyInfoBO surveyInfoBO)
        {
            surveyInfoBO.SurveyId = formDigest.FormId;
            surveyInfoBO.SurveyName = formDigest.FormName;
            surveyInfoBO.OrganizationName = formDigest.OrganizationName;
            surveyInfoBO.OwnerId = formDigest.OwnerUserId;
            surveyInfoBO.ParentId = formDigest.ParentFormId;
            surveyInfoBO.ViewId = formDigest.ViewId;
            surveyInfoBO.DataAccessRuleId = formDigest.DataAccessRuleId;
            surveyInfoBO.IsDraftMode = formDigest.IsDraftMode;
            return surveyInfoBO;
        }

        public static FormsHierarchyBO ToFormsHierarchyBO(this FormDigest formDigest, SurveyInfoBO surveyInfoBO)
        {
            var formsHierarchyBO = new FormsHierarchyBO { SurveyInfo = surveyInfoBO, IsSqlProject = true };
            return formDigest.MergeIntoFormsHierarchyBO(formsHierarchyBO);
        }

        public static List<FormsHierarchyBO> ToFormsHierarchyBOList(this FormDigest[] formDigests, SurveyInfoBO surveyInfoBO)
        {
            List<FormsHierarchyBO> formsHierarchyBOs = formDigests.Select(d => d.ToFormsHierarchyBO(surveyInfoBO)).ToList();
            return formsHierarchyBOs;
        }

        public static FormsHierarchyBO MergeIntoFormsHierarchyBO(this FormDigest formDigest, FormsHierarchyBO formsHierarchyBO)
        {
            formsHierarchyBO.FormId = formDigest.FormId;
            formsHierarchyBO.ViewId = formDigest.ViewId;
            formsHierarchyBO.RootFormId = formDigest.RootFormId;
            formsHierarchyBO.IsRoot = formDigest.FormId == formDigest.RootFormId;
            return formsHierarchyBO;
        }

        public static FormsHierarchyDTO ToFormsHierarchyDTO(this FormDigest formDigest, SurveyInfoDTO surveyInfoDTO)
        {
            var formsHierarchyDTO = new FormsHierarchyDTO { SurveyInfo = surveyInfoDTO, IsSqlProject = true };
            return formDigest.MergeIntoFormsHierarchyDTO(formsHierarchyDTO);
        }

        public static List<FormsHierarchyDTO> ToFormsHierarchyDTOList(this FormDigest[] formDigests, SurveyInfoDTO surveyInfoDTO)
        {
            List<FormsHierarchyDTO> formsHierarchyDTOs = formDigests.Select(d => d.ToFormsHierarchyDTO(surveyInfoDTO)).ToList();
            return formsHierarchyDTOs;
        }

        public static FormsHierarchyDTO MergeIntoFormsHierarchyDTO(this FormDigest formDigest, FormsHierarchyDTO formsHierarchyDTO, SurveyInfoDTO surveyInfoDTO = null)
        {
            formsHierarchyDTO.FormId = formDigest.FormId;
            formsHierarchyDTO.ViewId = formDigest.ViewId;
            formsHierarchyDTO.RootFormId = formDigest.RootFormId;
            formsHierarchyDTO.IsRoot = formDigest.FormId == formDigest.RootFormId;
            if (surveyInfoDTO != null) formsHierarchyDTO.SurveyInfo = surveyInfoDTO;
            return formsHierarchyDTO;
        }
    }
}
