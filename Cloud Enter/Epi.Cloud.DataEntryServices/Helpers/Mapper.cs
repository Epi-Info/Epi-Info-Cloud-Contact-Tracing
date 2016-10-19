using System.Collections.Generic;
using Epi.DataPersistence.DataStructures;
using Epi.Web.Enter.Common.BusinessObject;

namespace Epi.Cloud.DataEntryServices.Helpers
{
    public static class Mapper
    {
        /// <summary>
        /// Maps SurveyMetaData entity to SurveyInfoBO business object.
        /// </summary>
        /// <param name="surveyResponse">A SurveyMetaData entity to be transformed.</param>
        /// <returns>A SurveyInfoBO business object.</returns>
        public static SurveyResponseBO Map(SurveyResponse surveyResponse, Web.EF.User user = null, int LastActiveUserId = -1)
        {
            SurveyResponseBO surveyResponseBO = new SurveyResponseBO();

            surveyResponseBO.SurveyId = surveyResponse.SurveyId.ToString();
            surveyResponseBO.ResponseId = surveyResponse.ResponseId.ToString();
            //surveyResponseBO.XML = entity.ResponseXML;
            surveyResponseBO.Status = surveyResponse.StatusId;
            surveyResponseBO.DateUpdated = surveyResponse.DateUpdated;
            surveyResponseBO.DateCompleted = surveyResponse.DateCompleted;
            //surveyResponseBO.TemplateXMLSize = (long?)entity.ResponseXMLSize;
            surveyResponseBO.DateCreated = surveyResponse.DateCreated;
            surveyResponseBO.IsDraftMode = surveyResponse.IsDraftMode;
            surveyResponseBO.IsLocked = surveyResponse.IsLocked;
            surveyResponseBO.LastActiveUserId = LastActiveUserId;
            //TODO Implement for DocumentDB
            //if (surveyResponse.SurveyMetaData != null)
            //{
            //    SurveyResponseBO.ViewId = (int)surveyResponse.SurveyMetaData.ViewId;
            //}
            if (surveyResponse.ParentRecordId != null)
            {
                surveyResponseBO.ParentRecordId = surveyResponse.ParentRecordId.ToString();
            }
            if (surveyResponse.RelateParentId != null)
            {
                surveyResponseBO.RelateParentId = surveyResponse.RelateParentId.ToString();
            }
            if (user != null)
            {
                surveyResponseBO.UserEmail = user == null ? string.Empty : user.EmailAddress;
            }
            surveyResponseBO.ResponseDetail = surveyResponse.ResponseDetail;

            return surveyResponseBO;
        }

        public static List<SurveyResponseBO> Map(List<SurveyResponse> entities)
        {
            List<SurveyResponseBO> result = new List<SurveyResponseBO>();
            foreach (var surveyResponse in entities)
            {
                result.Add(Map(surveyResponse));
            }

            return result;
        }
    }
}
