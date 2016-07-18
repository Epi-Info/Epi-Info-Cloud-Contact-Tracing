using System.Collections.Generic;
using Epi.Web.Enter.Common.DTO;
using MvcDynamicForms;
using Epi.Cloud.DataEntryServices.Model;
using System;
using System.Threading.Tasks;
using Epi.Web.Enter.Common.Message;
using Epi.Cloud.Common.Metadata;

namespace Epi.Cloud.DataEntryServices.Facade
{
    public class SurveyDocumentDBFacade : ISurveyStoreDocumentDBFacade
    {
        /// <summary>
        /// Insert survey question and answer to Document Db
        /// </summary>

        #region Insert Into Servey Response to DocumentDB
        public async Task<bool> InsertSurveyResponseToDocumentDBStoreAsync(SurveyInfoModel surveyInfoModel, string responseId, Form form, SurveyAnswerDTO surveyAnswerDTO, bool IsSubmited, bool IsSaved, int PageNumber, int UserId)
        {
            CRUDSurveyResponse _surveyResponse = new CRUDSurveyResponse();
            Survey _storeSurvey = new Survey();

            _storeSurvey.SurveyName = surveyInfoModel.SurveyName;

            SurveyProperties _surveyResponseData = new SurveyProperties()
            {
                RecStatus = form.StatusId,
                SurveyID = surveyInfoModel.SurveyId,
                GlobalRecordID = responseId,
                FirstSaveTime = DateTime.UtcNow,
                LastSaveTime = DateTime.UtcNow,
                PageId = form.PageId,
                UserId = UserId.ToString()
            };

            _storeSurvey.SurveyProperties = _surveyResponseData;

            _storeSurvey.SurveyQuestionandAnswer = ReadQuestionandAnswerFromAllPage(form, _storeSurvey, surveyInfoModel, responseId);
            bool response = await _surveyResponse.InsertToSurveyToDocumentDBAsync(_storeSurvey);
            return true;
        }
        #endregion

        #region Read Question and Answer from all pages
        public SurveyQuestionandAnswer ReadQuestionandAnswerFromAllPage(Form form, Survey _surveyInfo, SurveyInfoModel surveyInfoModel, string responseId)
        {

            SurveyQuestionandAnswer _surveyQA = new SurveyQuestionandAnswer();

            _surveyQA.Digest = new List<Digest>();
            Digest _digest = new Digest();
            _digest.Fields = new Dictionary<string, string>();

            foreach (var item in form.SurveyInfo.ProjectTemplateMetadata.Project.Digest)
            {
                if (item.PageId == Convert.ToInt32(form.PageId))
                {
                    _digest.FormId = item.FormId;
                    _digest.FormName = item.FormName;
                    _digest.PageId = item.PageId;
                    _digest.Position = item.Position;
                    _digest.ViewId = item.ViewId;

                }
            }
            foreach (var field in form.InputFields)
            {
                if (!field.IsPlaceHolder)
                {
                    _digest.Fields.Add(field.Key, field.Response);
                }
            }
            _surveyQA.Digest.Add(_digest);
            _surveyQA.GlobalRecordID = responseId;
            _surveyQA.PageId = Convert.ToInt32(form.PageId);
            return _surveyQA;
        }
        #endregion

        #region ReadSurveyInfromDocumentDb
        public SurveyQuestionandAnswer ReadSurveyInfromDocumentDocumentDB(string responseId, string PageNumber)
        {
            CRUDSurveyResponse _surveyResponse = new CRUDSurveyResponse();
            SurveyQuestionandAnswer SurveyResponse = new SurveyQuestionandAnswer();
            SurveyProperties _surveyProperties = new SurveyProperties();
            _surveyProperties.GlobalRecordID = responseId;
            _surveyProperties.PageId = PageNumber;
            // SurveyResponse=_surveyResponse.ReadSurveyFromDocumentDB(_surveyProperties, SurveyName);
            return SurveyResponse;
        }
        #endregion

        #region ReadSurveyAnswerByResponseID,PageId 
        public SurveyQuestionandAnswer ReadSurveyAnswerByResponseID(string surveyName, string surveyId, string responseId, string pageId)
        {
            return ReadSurveyAnswerByResponseIDAsync(surveyName, surveyId, responseId, pageId).Result;
        }

        public async Task<SurveyQuestionandAnswer> ReadSurveyAnswerByResponseIDAsync(string surveyName, string surveyId, string responseId, string pageId)
        {
            CRUDSurveyResponse crudSurveyResponse = new CRUDSurveyResponse();
            //surveyResponse.SurveyQAList = _surveyResponse.ReadSruveyFromDocumentDBByPageandRespondId(databaseName,responseId,pageId);

            SurveyQuestionandAnswer surveyResponse = await crudSurveyResponse.ReadSurveyFromDocumentDBByPageandRespondIdAsync(surveyName, responseId, pageId);

            return surveyResponse;
        }

        #endregion

        #region DeleteSurveyByResponseId
        public SurveyAnswerResponse DeleteResponse(Survey SARequest)
        {
            SurveyAnswerResponse surveyAnsResponse = new SurveyAnswerResponse();
            CRUDSurveyResponse _surveyResponse = new CRUDSurveyResponse();
            var tasks = _surveyResponse.DeleteDocumentByIdAsync(SARequest);
            var result = tasks.Result;
            return surveyAnsResponse;
        }

        #endregion



        #region Save SurveyQuestionAnswer to DocumentDB

        public bool SaveSurveyAnswerToDocumentDB(ProjectDigest[] ProjectMetaData, int Status, int UserId, string ResponseId)
        {
            CRUDSurveyResponse _surveyResponse = new CRUDSurveyResponse();
            Survey _storeSurvey = new Survey();
            SurveyProperties _surveyResponseData = new SurveyProperties()
            {
                RecStatus = Status,
                SurveyID = ProjectMetaData[0].FormId,
                GlobalRecordID = ResponseId,
                FirstSaveTime = DateTime.UtcNow,
                LastSaveTime = DateTime.UtcNow,
                UserId = UserId.ToString()
            };

            _storeSurvey.SurveyProperties = new SurveyProperties();
            _storeSurvey.SurveyProperties = _surveyResponseData;
            _storeSurvey.SurveyName = ProjectMetaData[0].FormName;
            _storeSurvey.SurveyQuestionandAnswer = new SurveyQuestionandAnswer();
            _storeSurvey.SurveyQuestionandAnswer.ProjectDigest = ProjectMetaData;
            _storeSurvey.SurveyQuestionandAnswer.GlobalRecordID = ResponseId;
            _storeSurvey.SurveyQuestionandAnswer.SurveyID = ProjectMetaData[0].FormId;

            var DocumentDBResponse = _surveyResponse.SaveSurveyQuestionInDocumentDBAsync(_storeSurvey);

            //  bool response = DocumentDBResponse.Result;
            return true;
        }
        #endregion
    }
}