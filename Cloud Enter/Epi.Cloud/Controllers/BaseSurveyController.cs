using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using Epi.Cloud.Common.DTO;
using Epi.Cloud.Common.Extensions;
using Epi.Cloud.Common.Message;
using Epi.Cloud.Common.Metadata;
using Epi.Cloud.Facades.Interfaces;
using Epi.Cloud.MVC.Constants;
using Epi.Cloud.MVC.Models;
using Epi.Cloud.MVC.Utility;
using Epi.Common.Attributes;
using Epi.Common.Core.DataStructures;
using Epi.Common.Core.Interfaces;
using Epi.Common.Security;
using Epi.FormMetadata.DataStructures;
using Newtonsoft.Json;

namespace Epi.Cloud.MVC.Controllers
{
    public abstract class BaseSurveyController : Controller
    {
        protected ISurveyFacade _surveyFacade;
        protected Epi.Cloud.Interfaces.MetadataInterfaces.IProjectMetadataProvider _projectMetadataProvider;

        protected List<KeyValuePair<int, string>> _columns = new List<KeyValuePair<int, string>>();
        protected List<KeyValuePair<int, FieldDigest>> _columnDigests = new List<KeyValuePair<int, FieldDigest>>();

        private MetadataAccessor _metadataAccessor;
        protected MetadataAccessor MetadataAccessor { get { return _metadataAccessor = _metadataAccessor ?? new MetadataAccessor(); } }
        protected int Compare(KeyValuePair<int, string> a, KeyValuePair<int, string> b)
        {
            return a.Key.CompareTo(b.Key);
        }

        protected ResponseContext GetSetResponseContext(string responseId)
        {
            ResponseContext responseContext = GetSessionValue<ResponseContext>(UserSession.Key.ResponseContext);
            if (responseContext == null)
            {
                responseContext = (ResponseContext)new ResponseContext
                {
                    RootFormId = GetStringSessionValue(UserSession.Key.RootFormId),
                    UserName = GetStringSessionValue(UserSession.Key.UserName)
                }.ResolveMetadataDependencies();
            }
            responseContext.ResponseId = responseId;
            responseContext.UserOrgId = GetIntSessionValue(UserSession.Key.CurrentOrgId);

            SetSessionValue(UserSession.Key.ResponseContext, responseContext);

            responseContext.UserId = GetIntSessionValue(UserSession.Key.UserId);
            return responseContext;
        }

        protected int Compare(KeyValuePair<int, FieldDigest> a, KeyValuePair<int, FieldDigest> b)
        {
            return a.Key.CompareTo(b.Key);
        }

        protected FormResponseInfoModel GetFormResponseInfoModel(string surveyId, int orgId, int userId)
        {
            FormResponseInfoModel formResponseInfoModel = new FormResponseInfoModel();
            formResponseInfoModel.SearchModel = new SearchBoxModel();
            var surveyResponseBuilder = new SurveyResponseBuilder();
            FormSettingRequest formSettingRequest = new FormSettingRequest { ProjectId = GetStringSessionValue(UserSession.Key.ProjectId) };

            //Populating the request

            formSettingRequest.FormInfo.FormId = surveyId;
            formSettingRequest.FormInfo.UserId = userId;
            //Getting Column Name  List
            formSettingRequest.CurrentOrgId = orgId;
            var formSettingResponse = formResponseInfoModel.FormSettingResponse = _surveyFacade.GetFormSettings(formSettingRequest);
            formSettingResponse.FormSetting.FormId = surveyId;
            _columns = formSettingResponse.FormSetting.ColumnNameList.ToList();
            _columns.Sort(Compare);
            _columnDigests = formSettingResponse.FormSetting.ColumnDigestList.ToList();
            _columnDigests.Sort(Compare);

            // Setting  Column Name  List
            formResponseInfoModel.Columns = _columns;
            formResponseInfoModel.ColumnDigests = _columnDigests;

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

        protected SurveyAnswerDTO GetSurveyAnswerState(IResponseContext responseContext)
        {
            var surveyAnswerRequest = new SurveyAnswerRequest { ResponseContext = responseContext.CloneResponseContext() };
            SurveyAnswerDTO result = _surveyFacade.GetSurveyAnswerDTO(surveyAnswerRequest);
            return result;
        }

        protected SurveyAnswerDTO GetSurveyAnswer(string responseId, string formId = "")
        {
            SurveyAnswerDTO result = null;

            string rootResponseId = GetStringSessionValue(UserSession.Key.RootResponseId);
            string rootFormId = GetStringSessionValue(UserSession.Key.RootFormId);
            int userId = GetIntSessionValue(UserSession.Key.UserId);
            int orgId = GetIntSessionValue(UserSession.Key.CurrentOrgId);
            string userName = GetStringSessionValue(UserSession.Key.UserName);

            ResponseContext responseContext = (ResponseContext)new ResponseContext
            {
                RootFormId = rootFormId,
                FormId = !string.IsNullOrEmpty(formId) ? formId : null,
                ResponseId = responseId,
                RootResponseId = rootResponseId,
                UserOrgId = orgId,
                UserId = userId,
                UserName = userName
            }.ResolveMetadataDependencies();

            var surveyAnswerRequest = new SurveyAnswerRequest { ResponseContext = responseContext };

            result = _surveyFacade.GetSurveyAnswerDTO(surveyAnswerRequest);
            result.FormOwnerId = MetadataAccessor.GetFormDigest(formId).OwnerUserId;

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

        protected string CreateSearchCriteria(NameValueCollection nameValueCollection, SearchBoxModel searchModel, FormResponseInfoModel model)
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

        /* --------------------------------------------------------------------------------------- */
        /*                                      Session Helpers                                    */
        /* --------------------------------------------------------------------------------------- */
        protected bool IsValueEncrypted(string key)
        {
            return SessionHelper.IsValueEncrypted(key);
        }

        protected bool IsSessionValueNull(string key)
        {
            return UserSession.IsSessionValueNull(Session, key);
        }

        protected bool GetBoolSessionValue(string key, bool? defaultValue = null, bool decryptIfEncrypted = true)
        {
            return UserSession.GetBoolSessionValue(Session, key, defaultValue, decryptIfEncrypted);
        }

        protected int GetIntSessionValue(string key, int? defaultValue = null, bool decryptIfEncrypted = true)
        {
            return UserSession.GetIntSessionValue(Session, key, defaultValue, decryptIfEncrypted);
        }

        protected string GetStringSessionValue(string key, string defaultValue = "~~~", bool decryptIfEncrypted = true)
        {
            return UserSession.GetStringSessionValue(Session, key, defaultValue, decryptIfEncrypted);
        }

        protected object GetSessionValue(string key, object defaultValue = null)
        {
            return UserSession.GetSessionValue(Session, key, defaultValue);
        }

        protected T GetSessionValue<T>(string key, T defaultValue = default(T)) where T : new()
        {
            return UserSession.GetSessionValue<T>(Session, key, defaultValue);
        }

        protected void SetSessionValue<T>(string key, T value, bool dontEncrypt = false)
        {
            UserSession.SetSessionValue<T>(Session, key, value, dontEncrypt);
        }

        protected void SetSessionObjectValue<T>(string key, T value) where T : new()
        {
            UserSession.SetSessionObjectValue<T>(Session, key, value);
        }

        protected void RemoveSessionValue(string key)
        {
            UserSession.RemoveSessionValue(Session, key);
        }

        protected void ClearSession()
        {
            UserSession.ClearSession(Session);
        }
    }
}
