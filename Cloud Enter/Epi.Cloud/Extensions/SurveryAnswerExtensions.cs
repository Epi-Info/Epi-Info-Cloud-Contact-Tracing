using System;
using System.Collections.Generic;
using Epi.Cloud.Common.DTO;
using Epi.Cloud.MVC.Models;
using Epi.Cloud.Common.Constants;

namespace Epi.Cloud.MVC.Extensions
{
    public static class SurveryAnswerExtensions
    {
        public static Epi.Cloud.MVC.Models.ResponseModel ToResponseModel(this SurveyAnswerDTO item, List<KeyValuePair<int, string>> Columns)
        {
            ResponseModel ResponseModel = new ResponseModel();

            var metadataColumns = Epi.Cloud.Common.Constants.Constant.MetadataColumnNames();

            try
            {
                ResponseModel.Column0 = item.ResponseId;
                ResponseModel.IsLocked = item.IsLocked;

                var responseQA = item.ResponseDetail.FlattenedResponseQA(key => key.ToLowerInvariant());
                string value;
                string _key = string.Empty;
                var columnsCount = Columns.Count;
                for (int i = 0; i < Constant.MaxGridColumns; ++i)
                {

                    if (i >= columnsCount)
                    {
                        // set value to empty string for unspecified columns
                        value = string.Empty;
                    }
                    else if (metadataColumns.Contains(Columns[i].Value))
                    {
                        // set value to value of special column
                        value = GetColumnValue(item, Columns[i].Value);
                    }
                    else
                    {
                        KeyValuePair<int, string> Column = Columns[i];
                        _key = Column.Value.ToLower();
                        value = responseQA.ContainsKey(Column.Value.ToLower()) ? responseQA[Column.Value.ToLower()] : string.Empty;
                    }

                    // set the associated ResponseModel column
                    switch (i)
                    {
                        case 0:
                            ResponseModel.Column1 = value;
                            ResponseModel.Column1Key = _key;
                            break;
                        case 1:
                            ResponseModel.Column2 = value;
                            ResponseModel.Column2Key = _key;
                            break;
                        case 2:
                            ResponseModel.Column3 = value;
                            ResponseModel.Column3Key = _key;
                            break;
                        case 3:
                            ResponseModel.Column4 = value;
                            ResponseModel.Column4Key = _key;
                            break;
                        case 4:
                            ResponseModel.Column5 = value;
                            ResponseModel.Column5Key = _key;
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

        private static string GetColumnValue(SurveyAnswerDTO item, string columnName)
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
