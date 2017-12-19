using System.Collections.Generic;
using System.Linq;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.DTO;
using Epi.Cloud.Common.Metadata;
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

        public static SurveyAnswerDTO ToSurveyAnswerDTO(this FormResponseDetail formResponseDetail, MetadataAccessor metadataAccessor = null)
        {
            SurveyAnswerDTO surveyAnswerDTO = new SurveyAnswerDTO(formResponseDetail);
            if (metadataAccessor == null) metadataAccessor = new MetadataAccessor();
            var formDigest = metadataAccessor.GetFormDigest(formResponseDetail.FormId);
            surveyAnswerDTO.ViewId = formDigest.ViewId;
            surveyAnswerDTO.RequestedViewId = surveyAnswerDTO.ViewId.ToString();
            surveyAnswerDTO.FormOwnerId = formDigest.OwnerUserId;
            return surveyAnswerDTO;
        }

    }
}
