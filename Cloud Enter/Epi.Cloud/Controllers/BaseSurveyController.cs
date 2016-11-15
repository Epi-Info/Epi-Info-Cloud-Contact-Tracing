using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Epi.Cloud.Common.Metadata;
using Epi.Cloud.Facades.Interfaces;
using Epi.FormMetadata.DataStructures;
using Epi.Web.Enter.Common.DTO;
using Epi.Web.Enter.Common.Message;
using Epi.Cloud.Common.Constants;
using Epi.Web.MVC.Models;
using Epi.Web.MVC.Utility;

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
            FormSettingRequest formSettingReq = new Enter.Common.Message.FormSettingRequest { ProjectId = Session[SessionKeys.ProjectId] as string };

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
    }
}
