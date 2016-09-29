using System.Collections.Generic;
using System.Linq;
using Epi.FormMetadata.DataStructures;
using Epi.Web.Enter.Common.BusinessObject;

namespace Epi.Cloud.MetadataServices.Extensions
{
    public static class FormDigestExtensions
    {
        public static SurveyInfoBO ToSurveyInfoBO(this FormDigest formDigest)
        {
            var surveyInfoBO = new SurveyInfoBO();
            return formDigest.MergeIntoSurveyInfoBO(surveyInfoBO);
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
        public static List<SurveyInfoBO> ToSurveyInfoBOList(this FormDigest[] formDigests)
        {
            List<SurveyInfoBO> surveyInfoBOs = formDigests.Select(d => d.ToSurveyInfoBO()).ToList();
            return surveyInfoBOs;
        }
    }
}
