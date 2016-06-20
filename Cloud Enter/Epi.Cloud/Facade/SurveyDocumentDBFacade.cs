using System.Collections.Generic;
using Epi.Web.Enter.Common.DTO;
using Epi.Web.MVC.Models;
using MvcDynamicForms;
using Epi.Cloud.DataEntryServices;
using Epi.Cloud.DataEntryServices.Model;
using System;
using System.Threading.Tasks;

namespace Epi.Web.MVC.Facade
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
                PageId = form.PageId,
                PagePosition = "0",
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow
            };

            _storeSurvey.SurveyProperties = _surveyResponseData;

            _storeSurvey.SurveyQuestionandAnswer = ReadQuestionandAnswerFromAllPage(form, _storeSurvey, surveyInfoModel, responseId);
            bool response = await _surveyResponse.InsertToSurveyToDocumentDB(_storeSurvey);
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
            SurveyQuestionandAnswer surveyResponse = new SurveyQuestionandAnswer();
            //surveyResponse.SurveyQAList = _surveyResponse.ReadSruveyFromDocumentDBByPageandRespondId(databaseName,responseId,pageId);

            surveyResponse = await crudSurveyResponse.ReadSurveyFromDocumentDBByPageandRespondIdAsync(surveyName, responseId, pageId);
            if (surveyResponse != null)
            {
                surveyResponse.SurveyQAList = surveyResponse.SurveyQAList;
            }

            return surveyResponse;
        }

        #endregion

    }
}