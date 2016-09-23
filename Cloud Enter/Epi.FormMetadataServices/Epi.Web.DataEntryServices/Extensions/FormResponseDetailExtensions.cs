using Epi.Cloud.Common.EntityObjects;
using Epi.Web.Enter.Common.BusinessObject;

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
    }
}
