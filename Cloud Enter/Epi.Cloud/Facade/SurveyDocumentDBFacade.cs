using System.Collections.Generic;
using Epi.Web.Enter.Common.DTO;
using Epi.Web.MVC.Models;
using MvcDynamicForms;
using Epi.Cloud.DataEntryServices;
using Epi.Cloud.DataEntryServices.Model;
using MvcDynamicForms.Fields;

namespace Epi.Web.MVC.Facade
{
    public class SurveyDocumentDBFacade : ISurveyStoreDocumentDBFacade
    {
        /// <summary>
        /// Insert survey question and answer to Document Db
        /// </summary>

        #region Insert Into Servey Response to DocumentDB
        public bool InsertSurveyResponseToDocumentDBStoreAsync(List<FieldAttributes> metadata, SurveyInfoModel surveyInfoModel, string responseId, Form form, SurveyAnswerDTO surveyAnswerDTO, bool IsSubmited, bool IsSaved, int PageNumber, int UserId)
        {
            CRUDSurveyResponse _surveyResponse = new CRUDSurveyResponse();
            Survey _storeSurvey = new Survey();

            _storeSurvey.SurveyName = surveyInfoModel.SurveyName;

            SurveyProperties _surveyResponseData = new SurveyProperties()
            {
                RecStatus = form.StatusId,
                SurveyID = surveyInfoModel.SurveyId,
                GlobalRecordID = responseId,
                PageId = form.PageId,
                PagePosition = "0"
            };

            _storeSurvey.SurveyProperties = _surveyResponseData;

            _storeSurvey.SurveyQuestionandAnswer = ReadQuestionandAnswerFromAllPage(form, _storeSurvey, surveyInfoModel, responseId);
            bool response = _surveyResponse.InsertToSruveyToDocumentDB(_storeSurvey);
            return true;
        }
        #endregion

        #region Read Question and Answer from all pages
        public SurveyQuestionandAnswer ReadQuestionandAnswerFromAllPage(Form form, Survey _surveyInfo, SurveyInfoModel surveyInfoModel, string responseId)
        {

            SurveyQuestionandAnswer _surveyQA = new SurveyQuestionandAnswer();
            _surveyQA.SurveyQAList = new Dictionary<string, string>();
            foreach (var field in form.InputFields)
            {
                if (!field.IsPlaceHolder)
                {
                    _surveyQA.SurveyQAList.Add(field.Key, field.Response);
                }
            }
            _surveyQA.GlobalRecordID = responseId;
            _surveyQA.PageId = form.PageId;
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
            // SurveyResponse=_surveyResponse.ReadSruveyFromDocumentDB(_surveyProperties, SurveyName);
            return SurveyResponse;
        }
        #endregion

        #region ReadSurveyAnswerByResponseID,PageId 
        public SurveyQuestionandAnswer ReadSurveyAnswerByResponseID(string suveyName, string surveyID, string responseID, string pageId)
        {
            CRUDSurveyResponse _surveyResponse = new CRUDSurveyResponse();
            SurveyQuestionandAnswer surveyResponse = new SurveyQuestionandAnswer();
            //surveyResponse.SurveyQAList = _surveyResponse.ReadSruveyFromDocumentDBByPageandRespondId(databaseName,responseId,pageId);
            var respnse = _surveyResponse.ReadSruveyFromDocumentDBByPageandRespondId(suveyName, responseID, pageId);
            surveyResponse.SurveyQAList = respnse.SurveyQAList;
            return surveyResponse;
        }
        #endregion

    }
}