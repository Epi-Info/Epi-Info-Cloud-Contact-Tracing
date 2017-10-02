using System.Collections.Generic;
using System.Linq;
using Epi.Cloud.Common.BusinessObjects;
using Epi.DataPersistence.DataStructures;

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

        public static List<SurveyResponseBO> ToSurveyResponseBOList(this IEnumerable<FormResponseDetail> formResponseDetailList)
        {
            var surveyResponseBOList = formResponseDetailList.Select(d => d.ToSurveyResponseBO()).ToList();
            return surveyResponseBOList;
        }

    }
}
