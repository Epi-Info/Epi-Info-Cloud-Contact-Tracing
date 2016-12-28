using Epi.DataPersistence.DataStructures;
using Epi.Cloud.Common.BusinessObjects;

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
                IsNewRecord = formResponseDetail.IsNewRecord,
                Status = formResponseDetail.RecStatus,
                ResponseDetail = formResponseDetail,
                ViewId = new Common.Metadata.MetadataAccessor().GetFormDigest(formResponseDetail.FormId).ViewId
            };
            return surveyResponseBO;
        }
	}
}
