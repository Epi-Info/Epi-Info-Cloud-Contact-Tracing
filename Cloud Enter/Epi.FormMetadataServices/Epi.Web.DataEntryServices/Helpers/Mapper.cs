using System.Collections.Generic;
using Epi.Cloud.Common.EntityObjects;
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
        public static SurveyResponseBO Map(SurveyResponse surveyResponse, Web.EF.User user = null, int LastActiveUseerId = -1)
        {
            SurveyResponseBO SurveyResponseBO = new SurveyResponseBO();

            SurveyResponseBO.SurveyId = surveyResponse.SurveyId.ToString();
            SurveyResponseBO.ResponseId = surveyResponse.ResponseId.ToString();
            //SurveyResponseBO.XML = entity.ResponseXML;
            SurveyResponseBO.Status = surveyResponse.StatusId;
            SurveyResponseBO.DateUpdated = surveyResponse.DateUpdated;
            SurveyResponseBO.DateCompleted = surveyResponse.DateCompleted;
            //SurveyResponseBO.TemplateXMLSize = (long?)entity.ResponseXMLSize;
            SurveyResponseBO.DateCreated = surveyResponse.DateCreated;
            SurveyResponseBO.IsDraftMode = surveyResponse.IsDraftMode;
            SurveyResponseBO.IsLocked = surveyResponse.IsLocked;
            SurveyResponseBO.LastActiveUserId = LastActiveUseerId;
            //TODO Implement for DocumentDB
            //if (surveyResponse.SurveyMetaData != null)
            //{
            //    SurveyResponseBO.ViewId = (int)surveyResponse.SurveyMetaData.ViewId;
            //}
            if (surveyResponse.ParentRecordId != null)
            {
                SurveyResponseBO.ParentRecordId = surveyResponse.ParentRecordId.ToString();
            }
            if (surveyResponse.RelateParentId != null)
            {
                SurveyResponseBO.RelateParentId = surveyResponse.RelateParentId.ToString();
            }
            //TODO Implement for DocumentDB
            //if (user != null)
            //{
            //    SurveyResponseBO.UserEmail = user == null ? string.Empty : user.EmailAddress;
            //}
            SurveyResponseBO.ResponseDetail = surveyResponse.ResponseDetail;

            return SurveyResponseBO;
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
