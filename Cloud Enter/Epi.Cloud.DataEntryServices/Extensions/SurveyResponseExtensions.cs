using System.Collections.Generic;
using System.Linq;
using Epi.DataPersistence.DataStructures;
using SurveyResponseBO = Epi.Cloud.Common.BusinessObjects.SurveyResponseBO;

namespace Epi.Cloud.DataEntryServices.Extensions
{
    public static class SurveyResponseExtensions
    {
        public static SurveyResponseBO ToSurveyResponseBO(this SurveyResponse surveyResponse, Web.EF.User user = null, int lastActiveUserId = -1)
        {
            SurveyResponseBO surveyResponseBO = new SurveyResponseBO();
            if (surveyResponseBO != null)
            {
                var surveyId = surveyResponse.SurveyId.ToString();
                surveyResponseBO.SurveyId = surveyId;
                surveyResponseBO.ResponseId = surveyResponse.ResponseId.ToString();
                surveyResponseBO.Status = surveyResponse.StatusId;
                surveyResponseBO.DateUpdated = surveyResponse.DateUpdated;
                surveyResponseBO.DateCompleted = surveyResponse.DateCompleted;
                surveyResponseBO.DateCreated = surveyResponse.DateCreated;
                surveyResponseBO.IsDraftMode = surveyResponse.IsDraftMode;
                surveyResponseBO.IsLocked = surveyResponse.IsLocked;
                surveyResponseBO.LastActiveUserId = lastActiveUserId;
                surveyResponseBO.ResponseDetail = surveyResponse.ResponseDetail;

                var metadataAccessor = new Epi.Cloud.Common.Metadata.MetadataAccessor(surveyId);
                surveyResponseBO.ViewId = metadataAccessor.GetFormDigest(surveyId).ViewId;

                if (surveyResponse.ParentResponseId != null)
                {
                    surveyResponseBO.ParentResponseId = surveyResponse.ParentResponseId.ToString();
                }
                if (surveyResponse.RelateParentId != null)
                {
                    surveyResponseBO.ParentResponseId = surveyResponse.RelateParentId.ToString();
                }
                if (user != null)
                {
                    surveyResponseBO.UserEmail = user == null ? string.Empty : user.EmailAddress;
                }
            }
            return surveyResponseBO;
        }
    }
}
