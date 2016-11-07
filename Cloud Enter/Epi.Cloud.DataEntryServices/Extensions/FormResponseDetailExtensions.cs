using Epi.DataPersistence.DataStructures;
using Epi.Web.Enter.Common.BusinessObject;
using static Epi.PersistenceServices.DocumentDB.DataStructures;

namespace Epi.Cloud.DataEntryServices.Extensions
{
    public static class FormResponseDetailExtensions
    {
        public static SurveyResponseBO ToSurveyResponseBO(this FormResponseDetail formResponseDetail)
        {
            var surveyResponseBO = new SurveyResponseBO
            {
                ResponseId = formResponseDetail.GlobalRecordID,
                SurveyId = formResponseDetail.FormId,
                DateCreated = formResponseDetail.FirstSaveTime,
                DateUpdated = formResponseDetail.LastSaveTime,
                IsDraftMode = formResponseDetail.IsDraftMode,
                LastActiveUserId = formResponseDetail.LastActiveUserId,
                RelateParentId = formResponseDetail.RelateParentResponseId,
                Status = formResponseDetail.RecStatus,
                ResponseDetail = formResponseDetail,
                ViewId = new Common.Metadata.MetadataAccessor().GetFormDigest(formResponseDetail.FormId).ViewId
            };
            return surveyResponseBO;
        }
		//public static FormResponseProperties ToFormResponseProperties(this FormResponseDetail formResponseDetail)
		//{
		//	FormResponseProperties formResponseProperties = new FormResponseProperties();
		//	formResponseProperties.GlobalRecordID = formResponseDetail.GlobalRecordID;
		//	formResponseProperties.FormId = formResponseDetail.FormId;
		//	formResponseProperties.FormName = formResponseDetail.FormName;
		//	formResponseProperties.RecStatus = formResponseDetail.RecStatus;
		//	formResponseProperties.RelateParentId = formResponseDetail.RelateParentResponseId;
		//	formResponseProperties.IsDraftMode = formResponseDetail.IsDraftMode;
		//	formResponseProperties.IsRelatedView = formResponseDetail.RelateParentResponseId != null;
		//	formResponseProperties.UserId = formResponseDetail.LastActiveUserId;
		//	return formResponseProperties;
		//}
	}
}
