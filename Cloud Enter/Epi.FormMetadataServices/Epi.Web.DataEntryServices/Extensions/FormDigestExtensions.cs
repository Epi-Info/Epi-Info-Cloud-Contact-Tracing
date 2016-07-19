using System;
using System.Collections.Generic;
using System.Linq;
using Epi.Cloud.Common.Metadata;
using Epi.Web.Enter.Common.BusinessObject;

namespace Epi.Cloud.DataEntryServices.Extensions
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
            //surveyInfoBO.OrganizationKey = new Guid(Epi.Web.Enter.Common.Security.Cryptography.Decrypt(formDigest.OrganizationKey));
            surveyInfoBO.OrganizationName = formDigest.OrganizationName;
            surveyInfoBO.OwnerId = formDigest.OwnerUserId;
            surveyInfoBO.ParentId = formDigest.ParentFormId;
            surveyInfoBO.ViewId = formDigest.ViewId;

            return surveyInfoBO;
        }
        public static List<SurveyInfoBO> ToSurveyInfoBOList(this FormDigest[] formDigests)
        {
            List<SurveyInfoBO> surveyInfoBOs = formDigests.Select(d => d.ToSurveyInfoBO()).ToList();
            return surveyInfoBOs;
        }
    }
}
