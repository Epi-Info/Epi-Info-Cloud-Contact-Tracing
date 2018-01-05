using Epi.Cloud.Common.DTO;
using Epi.Cloud.Common.Message;
using Epi.Cloud.Common.Metadata;
using Epi.Common.Core.DataStructures;
using Epi.Common.Core.Interfaces;
using Epi.DataPersistence.Constants;
using Epi.DataPersistence.DataStructures;

namespace Epi.Cloud.Common.Extensions
{
    public static class ResponseContextExtensions
    {
        static MetadataAccessor _metadataAccessor = new MetadataAccessor();

        public static IResponseContext ResolveMetadataDependencies(this IResponseContext responseContext)
        {
            if (!string.IsNullOrEmpty(responseContext.FormId))
            {
                responseContext.FormName = _metadataAccessor.GetFormName(responseContext.FormId);
                responseContext.ParentFormId = _metadataAccessor.GetParentFormId(responseContext.FormId);
                responseContext.RootFormId = _metadataAccessor.GetRootFormId(responseContext.FormId);
            }
            else if (!string.IsNullOrEmpty(responseContext.ParentFormId))
            {
                responseContext.RootFormId = _metadataAccessor.GetRootFormId(responseContext.ParentFormId);
            }
            else if (string.IsNullOrEmpty(responseContext.RootFormId))
            {
                // Get the first form Id that the MetadataAccessor is aware of.
                responseContext.RootFormId = _metadataAccessor.GetRootFormId();
            }

            if (!string.IsNullOrEmpty(responseContext.ParentFormId))
            {
                responseContext.ParentFormName = _metadataAccessor.GetFormName(responseContext.ParentFormId);
            }

            if (!string.IsNullOrEmpty(responseContext.RootFormId))
            {
                responseContext.RootFormName = _metadataAccessor.GetFormName(responseContext.RootFormId);
            }

            if (string.IsNullOrEmpty(responseContext.RootResponseId) || responseContext.FormId == responseContext.RootFormId) responseContext.RootResponseId = responseContext.ResponseId;


            if (responseContext.IsRootResponse)
            {
                responseContext.ResponseId = responseContext.ResponseId ?? responseContext.RootResponseId;
                responseContext.FormId = responseContext.FormId ?? responseContext.RootFormId;
                responseContext.FormName = responseContext.FormName ?? responseContext.RootFormName;
            }

            return responseContext;
        }

        public static ResponseContext CloneResponseContext(this IResponseContext responseContext)
        {
            return new ResponseContext
            {
                ResponseId = responseContext.ResponseId,
                FormId = responseContext.FormId,
                FormName = responseContext.FormName,


                ParentResponseId = responseContext.ParentResponseId,
                ParentFormId = responseContext.ParentFormId,
                ParentFormName = responseContext.ParentFormName,


                RootResponseId = responseContext.RootResponseId,
                RootFormId = responseContext.RootFormId,
                RootFormName = responseContext.RootFormName,

                IsNewRecord = responseContext.IsNewRecord,

                UserOrgId = responseContext.UserOrgId,
                UserId = responseContext.UserId,
                UserName = responseContext.UserName
            }.ResolveMetadataDependencies() as ResponseContext;
        }

        public static FormResponseDetail ToFormResponseDetail(this IResponseContext responseContext, int? pageNumber = null)
        {
            var formResponseDetail = new FormResponseDetail();

            formResponseDetail.IsNewRecord = true;
            formResponseDetail.RecStatus = RecordStatus.InProcess;
            formResponseDetail.LastPageVisited = pageNumber.HasValue ? pageNumber.Value : 1;

            formResponseDetail.ResponseId = responseContext.ResponseId;
            formResponseDetail.FormId = responseContext.FormId;
            formResponseDetail.FormName = responseContext.FormName;
            formResponseDetail.ParentResponseId = responseContext.ParentResponseId;
            formResponseDetail.ParentFormId = responseContext.ParentFormId;
            formResponseDetail.ParentFormName = responseContext.ParentFormName;
            formResponseDetail.RootResponseId = responseContext.RootResponseId;
            formResponseDetail.RootFormId = responseContext.RootFormId;
            formResponseDetail.RootFormName = responseContext.RootFormName;
            formResponseDetail.IsNewRecord = responseContext.IsNewRecord;

            formResponseDetail.UserOrgId = responseContext.UserOrgId;
            formResponseDetail.UserId = responseContext.UserId;
            formResponseDetail.UserName = responseContext.UserName;
            formResponseDetail.FirstSaveLogonName = responseContext.UserName;
            formResponseDetail.LastSaveLogonName = responseContext.UserName;

            var pageResponseDetail = new PageResponseDetail
            {
                PageNumber = formResponseDetail.LastPageVisited,
                PageId = _metadataAccessor.GetPageDigestByPageNumber(responseContext.FormId, formResponseDetail.LastPageVisited).PageId,
                HasBeenUpdated = true
            };
            formResponseDetail.AddPageResponseDetail(pageResponseDetail);
            return formResponseDetail;
        }

        public static SurveyAnswerDTO ToSurveyAnswerDTOLite(this IResponseContext responseContext, SurveyAnswerDTO surveyAnswerDTO = null, int? recStatus = null)
        {
            if (surveyAnswerDTO == null) surveyAnswerDTO = new SurveyAnswerDTO();
            surveyAnswerDTO.ResponseDetail = responseContext.ToFormResponseDetail();
            if (recStatus.HasValue) surveyAnswerDTO.RecStatus = recStatus.Value;
            return surveyAnswerDTO;
        }

        public static SurveyAnswerRequest ToSurveyAnswerRequest(this IResponseContext responseContext, SurveyAnswerRequest surveyAnswerRequest = null)
        {
            if (surveyAnswerRequest == null) surveyAnswerRequest = new SurveyAnswerRequest();
            surveyAnswerRequest.ResponseContext = responseContext;
            return surveyAnswerRequest;
        }
    }
}
