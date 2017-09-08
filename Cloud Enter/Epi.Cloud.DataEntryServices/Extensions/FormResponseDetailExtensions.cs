using Epi.DataPersistence.DataStructures;
using Epi.Cloud.Common.BusinessObjects;
using System.Collections;
using System.Linq;

namespace Epi.Cloud.DataEntryServices.Extensions
{
    public static class FormResponseDetailExtensions
    {
        public static SurveyResponseBO ToSurveyResponseBO(this FormResponseDetail formResponseDetail)
        {
            var surveyResponseBO = new SurveyResponseBO();
            surveyResponseBO.ResponseDetail = formResponseDetail;
            surveyResponseBO.ViewId = new Common.Metadata.MetadataAccessor().GetFormDigest(formResponseDetail.FormId).ViewId;
            return surveyResponseBO;
        }
	}
}
