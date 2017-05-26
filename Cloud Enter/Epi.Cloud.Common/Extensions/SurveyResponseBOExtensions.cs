using System.Collections.Generic;
using System.Linq;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.DTO;
using Epi.Cloud.Common.Metadata;
using Epi.Common.Core.DataStructures;

namespace Epi.Cloud.Common.Extensions
{
    public static class SurveyResponseBOExtensions
    {
        public static SurveyAnswerDTO ToSurveyAnswerDTO(this SurveyResponseBO surveyResponseBO)
        {
            return new SurveyAnswerDTO
            {
                ResponseId = surveyResponseBO.ResponseId,
                ParentResponseId = surveyResponseBO.ParentResponseId,
                RootResponseId = surveyResponseBO.RootResponseId,
                SurveyId = surveyResponseBO.FormId,
                DateUpdated = surveyResponseBO.DateUpdated,
                DateCompleted = surveyResponseBO.DateCompleted,
                DateCreated = surveyResponseBO.DateCreated,
                IsNewRecord = surveyResponseBO.IsNewRecord,
                Status = surveyResponseBO.Status,
                ReasonForStatusChange = surveyResponseBO.ReasonForStatusChange,
                UserPublishKey = surveyResponseBO.UserPublishKey,
                IsDraftMode = surveyResponseBO.IsDraftMode,
                IsLocked = surveyResponseBO.IsLocked,
                UserEmail = surveyResponseBO.UserEmail,
                LastActiveUserId = surveyResponseBO.LastActiveUserId,
                RecordSourceId = surveyResponseBO.RecordSourceId,
                ViewId = surveyResponseBO.ViewId,
                FormOwnerId = 0, // TODO: Add FormOwnerId
                LoggedInUserId = surveyResponseBO.UserId,
                RecoverLastRecordVersion = false, // TODO: Do we have to populate RecoverLastRecordVersion
                RequestedViewId = string.Empty,
                CurrentPageNumber = 0,
                ResponseDetail = surveyResponseBO.ResponseDetail
            };
        }
        public static List<SurveyAnswerDTO> ToSurveyAnswerDTOList(this List<SurveyResponseBO> surveyResponseBOList)
        {
            return surveyResponseBOList.Select(surveyResponseBO => surveyResponseBO.ToSurveyAnswerDTO()).ToList();
        }

        public static SurveyResponseBO MergeIntoSurveyResponseBO(this SurveyResponseBO surveyResponseBO, SurveyInfoBO parentSurveyInfoBO, string parentResponseId)
        {
            surveyResponseBO.ParentFormId = parentSurveyInfoBO.ParentFormId;
            surveyResponseBO.ParentResponseId = parentResponseId;
            surveyResponseBO.IsDraftMode = parentSurveyInfoBO.IsDraftMode;

            var responseDetail = surveyResponseBO.ResponseDetail;
            responseDetail.ParentFormId = parentSurveyInfoBO.ParentFormId;
            responseDetail.ParentResponseId = parentResponseId;
            responseDetail.IsRelatedView = surveyResponseBO.ParentFormId != null;
            responseDetail.IsDraftMode = parentSurveyInfoBO.IsDraftMode;
            responseDetail.IsNewRecord = surveyResponseBO.IsNewRecord;
            responseDetail.RecStatus = surveyResponseBO.Status;
            responseDetail.ResponseId = surveyResponseBO.ResponseId;
            responseDetail.ResolveMetadataDependencies();

            surveyResponseBO.RootFormId = responseDetail.RootFormId;
            surveyResponseBO.RootFormName = responseDetail.RootFormName;
            surveyResponseBO.RootResponseId = responseDetail.RootResponseId;

            surveyResponseBO.ParentFormName = responseDetail.ParentFormName;
            surveyResponseBO.FormName = responseDetail.FormName;

            return surveyResponseBO;
        }

        public static ResponseContext ToResponseContext(this SurveyResponseBO surveyResponseBO)
        {
            MetadataAccessor metadataAccessor = new MetadataAccessor();
            var formId = surveyResponseBO.FormId ?? surveyResponseBO.FormId;
            var formName = surveyResponseBO.FormName ?? metadataAccessor.GetFormName(formId);
            var parentFormId = surveyResponseBO.ParentFormId ?? metadataAccessor.GetParentFormId(formId);
            var parentFormName = surveyResponseBO.ParentFormName ?? metadataAccessor.GetParentFormName(formId);
            var rootFormId = surveyResponseBO.RootFormId ?? formId;
            var rootFormName = surveyResponseBO.RootFormName ?? formName;
            var responseContext = new ResponseContext
            {
                ResponseId = surveyResponseBO.ResponseId,
                ParentResponseId = surveyResponseBO.ParentResponseId,
                RootResponseId = surveyResponseBO.RootResponseId,
                FormId = formId,
                FormName = formName,
                ParentFormId = parentFormId,
                ParentFormName = parentFormName,
                RootFormId = rootFormId,
                RootFormName = rootFormName,
                IsNewRecord = surveyResponseBO.IsNewRecord,

                OrgId = surveyResponseBO.OrgId,
                UserId = surveyResponseBO.UserId,
                UserName = surveyResponseBO.UserName
            };

            //if (responseContext.FormId != metadataAccessor.GetFormIdByViewId(surveyResponseBO.ViewId))
            //    throw new ArgumentException("ViewId not in agreement with FormId", string.Format("FormId={0}, ViewId=>{1}",
            //        responseContext.FormId, metadataAccessor.GetFormIdByViewId(surveyResponseBO.ViewId)));

            return responseContext;
        }
    }
}
