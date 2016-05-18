using System.Collections.Generic;
using Epi.Web.Enter.Common.DTO;
using Epi.Web.MVC.Models;
using MvcDynamicForms;
using Epi.Cloud.DataEntryServices;
using Epi.Cloud.DataEntryServices.Model;

namespace Epi.Web.MVC.Facade
{
    public class SurveyDocumentDBFacade : ISurveyStoreDocumentDBFacade
    {
        /// <summary>
        /// Insert survey question and answer to Document Db
        /// </summary>

        /// <returns></returns>
        public bool InsertSurveyResponseToDocumentDBStoreAsync(SurveyInfoModel surveyInfoModel, string responseId, Form form, SurveyAnswerDTO surveyAnswerDTO, bool IsSubmited, bool IsSaved, int PageNumber, int UserId)
        {
            CRUDSurveyResponse _surveyResponse = new CRUDSurveyResponse();
            Survey _storeSurvey = new Survey();

            SurveyProperties _surveyResponseData = new SurveyProperties()
            {

                RecStatus = "1",
                GlobalRecordID = surveyInfoModel.SurveyId,

            };
            _storeSurvey.SurveyProperties = _surveyResponseData;
            _storeSurvey.SurveyQuestionandAnswer = ReadQuestionandAnswerFromAllPage(form, _storeSurvey);
            bool response = _surveyResponse.InsertToSruveyToDocumentDB(_storeSurvey);
            return true;
        }



        public List<SurveyQuestionandAnswer> ReadQuestionandAnswerFromAllPage(Form form, Survey _surveyInfo)
        {
            List<SurveyQuestionandAnswer> _surveyQAAllPages = new List<SurveyQuestionandAnswer>();

            SurveyQuestionandAnswer _surveyQA = new SurveyQuestionandAnswer();
            _surveyQA.SurveyQAList = new List<KeyValuePair<string, string>>();

            foreach (var field in form.InputFields)
            {
                if (!field.IsPlaceHolder)
                {
                    _surveyQA.SurveyQAList.Add(new KeyValuePair<string, string>(field.Title, field.Response));
                }
            }

            //Add all pages 
            _surveyQAAllPages.Add(_surveyQA);
            return _surveyQAAllPages;
        }
    }
}