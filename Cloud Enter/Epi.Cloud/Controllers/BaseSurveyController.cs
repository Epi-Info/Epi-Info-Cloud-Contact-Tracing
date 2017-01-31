using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Epi.Cloud.Common.Metadata;
using Epi.Cloud.Facades.Interfaces;
using Epi.FormMetadata.DataStructures;
using Epi.Cloud.Common.DTO;
using Epi.Cloud.Common.Message;
using Epi.Cloud.Common.Constants;
using Epi.Web.MVC.Models;
using Epi.Web.MVC.Utility;
using System;
using System.Text;

namespace Epi.Web.MVC.Controllers
{
    public abstract class BaseSurveyController : Controller
    {
        protected ISurveyFacade _surveyFacade;
        protected Epi.Cloud.Interfaces.MetadataInterfaces.IProjectMetadataProvider _projectMetadataProvider;

        protected List<KeyValuePair<int, string>> Columns = new List<KeyValuePair<int, string>>();
        protected List<KeyValuePair<int, FieldDigest>> ColumnDigests = new List<KeyValuePair<int, FieldDigest>>();

        private MetadataAccessor _metadataAccessor;
        protected MetadataAccessor MetadataAccessor { get { return _metadataAccessor = _metadataAccessor ?? new MetadataAccessor(); } }
        protected int Compare(KeyValuePair<int, string> a, KeyValuePair<int, string> b)
        {
            return a.Key.CompareTo(b.Key);
        }

        protected int Compare(KeyValuePair<int, FieldDigest> a, KeyValuePair<int, FieldDigest> b)
        {
            return a.Key.CompareTo(b.Key);
        }

        protected FormResponseInfoModel GetFormResponseInfoModel(string SurveyId, int orgid, int userId)
        {
            FormResponseInfoModel formResponseInfoModel = new FormResponseInfoModel();
            formResponseInfoModel.SearchModel = new SearchBoxModel();
            var surveyResponseHelper = new SurveyResponseHelper();
            FormSettingRequest formSettingReq = new FormSettingRequest { ProjectId = Session[SessionKeys.ProjectId] as string };

            //Populating the request

            formSettingReq.FormInfo.FormId = SurveyId;
            formSettingReq.FormInfo.UserId = userId;
            //Getting Column Name  List
            formSettingReq.CurrentOrgId = orgid;
            var formSettingResponse = formResponseInfoModel.FormSettingResponse = _surveyFacade.GetFormSettings(formSettingReq);
            formSettingResponse.FormSetting.FormId = SurveyId;
            Columns = formSettingResponse.FormSetting.ColumnNameList.ToList();
            Columns.Sort(Compare);
            ColumnDigests = formSettingResponse.FormSetting.ColumnDigestList.ToList();
            ColumnDigests.Sort(Compare);

            // Setting  Column Name  List
            formResponseInfoModel.Columns = Columns;
            formResponseInfoModel.ColumnDigests = ColumnDigests;

            return formResponseInfoModel;
        }

        protected ResponseModel ConvertRowToModel(SurveyAnswerDTO item, List<KeyValuePair<int, string>> columns, string globalRecordIdKey)
        {
            return ConvertRowToModel(item, columns.Select(c => c.Value).ToList(), globalRecordIdKey);
        }


        protected ResponseModel ConvertRowToModel(SurveyAnswerDTO item, List<string> columns, string globalRecordIdKey)
        {
            ResponseModel responseModel = new ResponseModel();

            responseModel.Column0 = item.SqlData[globalRecordIdKey];
            if (columns.Count > 0)
            {
                responseModel.Column1 = item.SqlData[columns[0]];
            }

            if (columns.Count > 1)
            {
                responseModel.Column2 = item.SqlData[columns[1]];
            }

            if (columns.Count > 2)
            {
                responseModel.Column3 = item.SqlData[columns[2]];
            }
            if (columns.Count > 3)
            {
                responseModel.Column4 = item.SqlData[columns[3]];
            }
            if (columns.Count > 4)
            {
                responseModel.Column5 = item.SqlData[columns[4]];
            }

            return responseModel;
        }



        protected SurveyAnswerStateDTO GetSurveyAnswerState(string responseId, string rootFormId = "", int userId = 0)
        {
            SurveyAnswerDTO result = null;
            int UserId = SurveyHelper.GetDecryptUserId(Session[SessionKeys.UserId].ToString());
            var surveyAnswerState = _surveyFacade.GetSurveyAnswerState(responseId, rootFormId, userId);
            result = surveyAnswerState.SurveyResponseList[0];
            return result;
        }

        public IDictionary<int, KeyValuePair<FieldDigest, string>> ToSearchDigestList(SearchBoxModel searchModel, string formId)
        {
            var searchDigestList = new Dictionary<int, KeyValuePair<FieldDigest, string>>();
            if (!string.IsNullOrWhiteSpace(searchModel.SearchCol1))
            {
                FieldDigest fieldDigest = MetadataAccessor.GetFieldDigestByFieldName(formId, searchModel.SearchCol1);
                searchDigestList.Add(1, new KeyValuePair<FieldDigest, string>(fieldDigest, searchModel.Value1));
            }
            if (!string.IsNullOrWhiteSpace(searchModel.SearchCol2))
            {
                FieldDigest fieldDigest = MetadataAccessor.GetFieldDigestByFieldName(formId, searchModel.SearchCol2);
                searchDigestList.Add(2, new KeyValuePair<FieldDigest, string>(fieldDigest, searchModel.Value2));
            }
            if (!string.IsNullOrWhiteSpace(searchModel.SearchCol3))
            {
                FieldDigest fieldDigest = MetadataAccessor.GetFieldDigestByFieldName(formId, searchModel.SearchCol3);
                searchDigestList.Add(3, new KeyValuePair<FieldDigest, string>(fieldDigest, searchModel.Value3));
            }
            if (!string.IsNullOrWhiteSpace(searchModel.SearchCol4))
            {
                FieldDigest fieldDigest = MetadataAccessor.GetFieldDigestByFieldName(formId, searchModel.SearchCol4);
                searchDigestList.Add(4, new KeyValuePair<FieldDigest, string>(fieldDigest, searchModel.Value4));
            }
            if (!string.IsNullOrWhiteSpace(searchModel.SearchCol5))
            {
                FieldDigest fieldDigest = MetadataAccessor.GetFieldDigestByFieldName(formId, searchModel.SearchCol5);
                searchDigestList.Add(5, new KeyValuePair<FieldDigest, string>(fieldDigest, searchModel.Value5));
            }
            return searchDigestList;
        }

        protected string CreateSearchCriteria(System.Collections.Specialized.NameValueCollection nameValueCollection, SearchBoxModel searchModel, FormResponseInfoModel model)
        {
            FormCollection formCollection = new FormCollection(nameValueCollection);

            StringBuilder searchBuilder = new StringBuilder();

            if (ValidateSearchFields(formCollection))
            {
                if (formCollection["col1"].Length > 0 && formCollection["val1"].Length > 0)
                {
                    searchBuilder.Append(formCollection["col1"] + "='" + formCollection["val1"] + "'");
                    searchModel.SearchCol1 = formCollection["col1"];
                    searchModel.Value1 = formCollection["val1"];
                }
                if (formCollection["col2"].Length > 0 && formCollection["val2"].Length > 0)
                {
                    searchBuilder.Append(" AND " + formCollection["col2"] + "='" + formCollection["val2"] + "'");
                    searchModel.SearchCol2 = formCollection["col2"];
                    searchModel.Value2 = formCollection["val2"];
                }
                if (formCollection["col3"].Length > 0 && formCollection["val3"].Length > 0)
                {
                    searchBuilder.Append(" AND " + formCollection["col3"] + "='" + formCollection["val3"] + "'");
                    searchModel.SearchCol3 = formCollection["col3"];
                    searchModel.Value3 = formCollection["val3"];
                }
                if (formCollection["col4"].Length > 0 && formCollection["val4"].Length > 0)
                {
                    searchBuilder.Append(" AND " + formCollection["col4"] + "='" + formCollection["val4"] + "'");
                    searchModel.SearchCol4 = formCollection["col4"];
                    searchModel.Value4 = formCollection["val4"];
                }
                if (formCollection["col5"].Length > 0 && formCollection["val5"].Length > 0)
                {
                    searchBuilder.Append(" AND " + formCollection["col5"] + "='" + formCollection["val5"] + "'");
                    searchModel.SearchCol5 = formCollection["col5"];
                    searchModel.Value5 = formCollection["val5"];
                }
            }

            return searchBuilder.ToString();
        }

        protected bool ValidateSearchFields(FormCollection formCollection)
        {
            if (string.IsNullOrEmpty(formCollection["col1"]) || formCollection["col1"] == "undefined" ||
               string.IsNullOrEmpty(formCollection["val1"]) || formCollection["val1"] == "undefined")
            {
                return false;
            }
            return true;
        }

        protected void PopulateDropDownlist(out List<SelectListItem> searchColumns, string selectedValue, List<KeyValuePair<int, string>> columns)
        {
            searchColumns = new List<SelectListItem>();
            foreach (var item in columns)
            {
                SelectListItem newSelectListItem = new SelectListItem { Text = item.Value, Value = item.Value, Selected = item.Value == selectedValue };
                searchColumns.Add(newSelectListItem);
            }
        }
    }
}
