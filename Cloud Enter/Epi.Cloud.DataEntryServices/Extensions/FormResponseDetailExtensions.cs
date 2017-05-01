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
                ResponseDetail = formResponseDetail,
                IsDraftMode = formResponseDetail.IsDraftMode,
                LastActiveUserId = formResponseDetail.UserId,
                IsNewRecord = formResponseDetail.IsNewRecord,
                Status = formResponseDetail.RecStatus,
                ViewId = new Common.Metadata.MetadataAccessor().GetFormDigest(formResponseDetail.FormId).ViewId
            };
            return surveyResponseBO;
        }
	}
}
