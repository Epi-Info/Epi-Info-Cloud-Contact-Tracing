using System;
using System.Collections.Generic;
using Epi.Web.Enter.Common.DTO;
using Epi.Web.MVC.Models;


namespace Epi.Cloud.MVC.Extensions
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
				ReasonForStatusChange = surveyAnswerDTO.ReasonForStatusChange,
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
            surveyAnswerDTO.ReasonForStatusChange = surveyAnswerStateDTO.ReasonForStatusChange;
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

        public static Epi.Web.MVC.Models.ResponseModel ToResponseModel(this SurveyAnswerDTO item, List<KeyValuePair<int, string>> Columns)
        {
            ResponseModel ResponseModel = new ResponseModel();

            var MetaDataColumns = Epi.Web.MVC.Constants.Constant.MetaDaTaColumnNames();

            try
            {
                ResponseModel.Column0 = item.ResponseId;
                ResponseModel.IsLocked = item.IsLocked;

                var responseQA = item.ResponseDetail.FlattenedResponseQA(key => key.ToLower());
                string value;
                var columnsCount = Columns.Count;
                for (int i = 0; i < 5; ++i)
                {
                    if (i >= columnsCount)
                    {
                        // set value to empty string for unspecified columns
                        value = string.Empty;
                    }
                    else if (MetaDataColumns.Contains(Columns[i].Value))
                    {
                        // set value to value of special column
                        value = GetColumnValue(item, Columns[i].Value);
                    }
                    else
                    {
                        // set value to value in the response
                        value = responseQA.TryGetValue(Columns[i].Value.ToLower(), out value) ? (value ?? string.Empty) : string.Empty;
                    }

                    // set the associated ResponseModel column
                    switch (i)
                    {
                        case 0:
                            ResponseModel.Column1 = value;
                            break;
                        case 1:
                            ResponseModel.Column2 = value;
                            break;
                        case 2:
                            ResponseModel.Column3 = value;
                            break;
                        case 3:
                            ResponseModel.Column4 = value;
                            break;
                        case 4:
                            ResponseModel.Column5 = value;
                            break;
                    }
                }

                return ResponseModel;
            }
            catch (Exception Ex)
            {

                throw new Exception(Ex.Message);
            }
        }

        private static string GetColumnValue(Epi.Web.Enter.Common.DTO.SurveyAnswerDTO item, string columnName)
        {
            string ColumnValue = "";
            switch (columnName)
            {
                case "_UserEmail":
                    ColumnValue = item.UserEmail;
                    break;
                case "_DateUpdated":
                    ColumnValue = item.DateUpdated.ToString();
                    break;
                case "_DateCreated":
                    ColumnValue = item.DateCreated.ToString();
                    break;
                case "IsDraftMode":
                case "_Mode":
                    if (item.IsDraftMode.ToString().ToUpper() == "TRUE")
                    {
                        ColumnValue = "Staging";
                    }
                    else
                    {
                        ColumnValue = "Production";

                    }
                    break;
            }
            return ColumnValue;
        }
    }
}
