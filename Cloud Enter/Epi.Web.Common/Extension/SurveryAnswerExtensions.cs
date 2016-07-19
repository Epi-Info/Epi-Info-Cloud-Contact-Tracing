using Epi.Web.Enter.Common.DTO;

namespace Epi.Web.Enter.Common.Extension
{
    public static class SurveryAnswerExtensions
    {
        public static SurveyAnswerStateDTO ToSurveyAnswerStateDTO(this SurveyAnswerDTO surveyAnswerDTO)
        {
            var surveyAnswerStateDTO = new SurveyAnswerStateDTO
            {
                ResponseId = surveyAnswerDTO.ResponseId,
                SurveyId = surveyAnswerDTO.SurveyId,
                DateUpdated = surveyAnswerDTO.DateUpdated,
                DateCompleted = surveyAnswerDTO.DateCompleted,
                DateCreated = surveyAnswerDTO.DateCreated,
                Status = surveyAnswerDTO.Status,
                UserPublishKey = surveyAnswerDTO.UserPublishKey,
                IsDraftMode = surveyAnswerDTO.IsDraftMode,
                IsLocked = surveyAnswerDTO.IsLocked,
                ParentRecordId = surveyAnswerDTO.ParentRecordId,
                UserEmail = surveyAnswerDTO.UserEmail,
                LastActiveUserId = surveyAnswerDTO.LastActiveUserId,
                RelateParentId = surveyAnswerDTO.RelateParentId,
                RecordSourceId = surveyAnswerDTO.RecordSourceId,
                ViewId = surveyAnswerDTO.ViewId,
                FormOwnerId = surveyAnswerDTO.FormOwnerId,
                LoggedInUserId = surveyAnswerDTO.LoggedInUserId,
                RecoverLastRecordVersion = surveyAnswerDTO.RecoverLastRecordVersion,
                RequestedViewId = surveyAnswerDTO.RequestedViewId,
                CurrentPageNumber = surveyAnswerDTO.CurrentPageNumber
            };
            return surveyAnswerStateDTO;
        }
        public static SurveyAnswerDTO ToSurveyAnswerDTO(this SurveyAnswerStateDTO surveyAnswerStateDTO)
        {
            var surveyAnswerDTO = new SurveyAnswerDTO();
            return surveyAnswerStateDTO.MergeIntoToSurveyAnswerDTO(surveyAnswerDTO);
        }

        public static SurveyAnswerDTO MergeIntoToSurveyAnswerDTO(this SurveyAnswerStateDTO surveyAnswerStateDTO, SurveyAnswerDTO surveyAnswerDTO)
        {
            surveyAnswerDTO.ResponseId = surveyAnswerStateDTO.ResponseId;
            surveyAnswerDTO.SurveyId = surveyAnswerStateDTO.SurveyId;
            surveyAnswerDTO.DateUpdated = surveyAnswerStateDTO.DateUpdated;
            surveyAnswerDTO.DateCompleted = surveyAnswerStateDTO.DateCompleted;
            surveyAnswerDTO.DateCreated = surveyAnswerStateDTO.DateCreated;
            surveyAnswerDTO.Status = surveyAnswerStateDTO.Status;
            surveyAnswerDTO.UserPublishKey = surveyAnswerStateDTO.UserPublishKey;
            surveyAnswerDTO.IsDraftMode = surveyAnswerStateDTO.IsDraftMode;
            surveyAnswerDTO.IsLocked = surveyAnswerStateDTO.IsLocked;
            surveyAnswerDTO.ParentRecordId = surveyAnswerStateDTO.ParentRecordId;
            surveyAnswerDTO.UserEmail = surveyAnswerStateDTO.UserEmail;
            surveyAnswerDTO.LastActiveUserId = surveyAnswerStateDTO.LastActiveUserId;
            surveyAnswerDTO.RelateParentId = surveyAnswerStateDTO.RelateParentId;
            surveyAnswerDTO.RecordSourceId = surveyAnswerStateDTO.RecordSourceId;
            surveyAnswerDTO.ViewId = surveyAnswerStateDTO.ViewId;
            surveyAnswerDTO.FormOwnerId = surveyAnswerStateDTO.FormOwnerId;
            surveyAnswerDTO.LoggedInUserId = surveyAnswerStateDTO.LoggedInUserId;
            surveyAnswerDTO.RecoverLastRecordVersion = surveyAnswerStateDTO.RecoverLastRecordVersion;
            surveyAnswerDTO.RequestedViewId = surveyAnswerStateDTO.RequestedViewId;
            surveyAnswerDTO.CurrentPageNumber = surveyAnswerStateDTO.CurrentPageNumber;
            return surveyAnswerDTO;
        }
    }
}
