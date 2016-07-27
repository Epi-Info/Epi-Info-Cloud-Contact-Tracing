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

            string Dbname = surveyInfoModel.SurveyName;
            //Get Collection Name
            _storeSurvey = GetDbAndCollectionName(form, Dbname);
            if (_storeSurvey.IsChildForm)
            {
                _storeSurvey.FormQuestionandAnswer = ReadQuestionandAnswerFromChildPage(form, _storeSurvey, surveyInfoModel, surveyAnswerDTO.RelateParentId, responseId);
            }
            else
            {
                _storeSurvey.FormQuestionandAnswer = ReadQuestionandAnswerFromPage(form, _storeSurvey, surveyInfoModel, responseId);
            }
            bool response = await _surveyResponse.InsertToSurveyToDocumentDBAsync(_storeSurvey);
            return true;
        }
        #endregion

        #region GetCollectionName
        public Survey GetDbAndCollectionName(Form form, string DbName)
        {
            Survey _survey = new Survey();
            _survey.SurveyName = form.SurveyInfo.ProjectTemplateMetadata.Project.Digest[0].FormName;
            foreach (var item in form.SurveyInfo.ProjectTemplateMetadata.Project.Digest)
            {
                if (item.PageId == Convert.ToInt32(form.PageId))
                {
                    _survey.IsChildForm = item.IsRelatedView;
                    _survey.CollectionName = DbName + item.PageId;
                    _survey.DatabaseName = DbName;
                }
            }
            return _survey;
        }
        #endregion

        #region Read Question and Answer from all pages
        public FormQuestionandAnswer ReadQuestionandAnswerFromPage(Form form, Survey _surveyInfo, SurveyInfoModel surveyInfoModel, string responseId)
        {

            FormQuestionandAnswer _surveyQA = new FormQuestionandAnswer();
            _surveyQA.SurveyQAList = new Dictionary<string, string>();
            foreach (var field in form.InputFields)
            {
                if (!field.IsPlaceHolder)
                {
                    _surveyQA.SurveyQAList.Add(field.Key, field.Response);
                }
            }
            _surveyQA.GlobalRecordID = responseId;
            return _surveyQA;
        }
        #endregion

        #region Read Question and Answer from child pages
        public FormQuestionandAnswer ReadQuestionandAnswerFromChildPage(Form form, Survey _surveyInfo, SurveyInfoModel surveyInfoModel, string relateParentId, string responseId)
        {

            FormQuestionandAnswer _surveyQA = new FormQuestionandAnswer();

            // _surveyQA.DigestQA = new List<Digest>();
            //Digest _digest = new Digest();
            //_digest.Fields = new Dictionary<string, string>();

            //foreach (var item in form.SurveyInfo.ProjectTemplateMetadata.Project.Digest)
            //{
            //    if (item.PageId == Convert.ToInt32(form.PageId))
            //    {
            //        _digest.FormId = item.FormId;
            //        _digest.FormName = item.FormName;
            //        _digest.PageId = item.PageId;
            //        _digest.Position = item.Position;
            //        _digest.ViewId = item.ViewId;
            //    }

            //}
            //foreach (var field in form.InputFields)
            //{
            //    if (!field.IsPlaceHolder)
            //    {
            //        _digest.Fields.Add(field.Key, field.Response);
            //    }
            //}
            //_digest.ChildRecStatus = 1;
            //_digest.ChildGlobalRecordID = responseId;

            //_surveyQA.DigestQA.Add(_digest);
            //_surveyQA.RelateParentId = relateParentId;


            // _surveyQA.PageId = Convert.ToInt32(form.PageId);
            return _surveyQA;
        }
        #endregion

        #region ReadSurveyInfromDocumentDb
        public FormQuestionandAnswer ReadSurveyInfromDocumentDocumentDB(string responseId, string PageNumber)
        {
            CRUDSurveyResponse _surveyResponse = new CRUDSurveyResponse();
            FormQuestionandAnswer SurveyResponse = new FormQuestionandAnswer();

            SurveyProperties _surveyProperties = new SurveyProperties();
            _surveyProperties.GlobalRecordID = responseId;
            //_surveyProperties.PageId = PageNumber;
            // SurveyResponse=_surveyResponse.ReadSurveyFromDocumentDB(_surveyProperties, SurveyName);
            return SurveyResponse;
        }
        #endregion

        #region ReadSurveyAnswerByResponseID,PageId 
        public FormQuestionandAnswer ReadSurveyAnswerByResponseID(string surveyName, string surveyId, string responseId, string pageId)
        {
            CRUDSurveyResponse crudSurveyResponse = new CRUDSurveyResponse();
            FormQuestionandAnswer formResponse = crudSurveyResponse.ReadSurveyFromDocumentDBByPageandRespondIdAsync(surveyName, responseId, pageId);
            return formResponse;


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

        #region Save FormParentProperties to DocumentDB
        /// <summary>
        /// First time store ResonseId,RecStatus, and SurveyId in DocumentDB
        /// </summary>
        /// <param name="ProjectMetaData"></param>
        /// <param name="Status"></param>
        /// <param name="UserId"></param>
        /// <param name="ResponseId"></param>
        /// <returns></returns>


        public FormsHierarchyDTO GetChildRecordByChildFormId(string ChildFormId, string RelateParentId, string DbName, List<string> Params)
        {
            CRUDSurveyResponse _surveyResponse = new CRUDSurveyResponse();
            FormsHierarchyDTO _formhierarchyDTO = new FormsHierarchyDTO();
            var GetChildRecords = _surveyResponse.ReadAllRecordByChildFormIdandRelateParentId(ChildFormId, RelateParentId, DbName, Params);
            _formhierarchyDTO.ResponseIds = new List<SurveyAnswerDTO>();
            SurveyAnswerDTO _surveyAnswer = new SurveyAnswerDTO();
            foreach (var item in GetChildRecords)
            {
                _surveyAnswer.ResponseId = item.ResponseId.ToString();
                _surveyAnswer.SqlData = item.ResponseQA;
            }
            _formhierarchyDTO.ResponseIds.Add(_surveyAnswer);
            return _formhierarchyDTO;
        }

        public bool SaveFormParentPropertiesToDocumentDB(ProjectDigest ProjectMetaData, bool Status, int UserId, string ResponseId)
        {

            CRUDSurveyResponse _surveyResponse = new CRUDSurveyResponse();
            Survey _storeForm = new Survey();
            FormParentProperties _FormData = new FormParentProperties()
            {
                //RecStatus = Status,
                FormId = ProjectMetaData.FormId,
                GlobalRecordID = ResponseId,
                IsRelatedView = ProjectMetaData.IsRelatedView,
                FirstSaveTime = DateTime.UtcNow,
                LastSaveTime = DateTime.UtcNow,
                UserId = UserId.ToString()
            };

            _storeForm.FormParentProperties = new FormParentProperties();
            _storeForm.FormParentProperties = _FormData;
            _storeForm.DatabaseName = ProjectMetaData.FormName;
            _storeForm.CollectionName = ProjectMetaData.FormName;
            var DocumentDBResponse = _surveyResponse.SaveSurveyQuestionInDocumentDBAsync(_storeForm);
            return true;
        }

        public bool SaveFormChildPropertiesToDocumentDB(ProjectDigest ProjectMetaData, bool Status, int UserId, string ResponseId)
        {

            CRUDSurveyResponse _surveyResponse = new CRUDSurveyResponse();
            Survey _storeSurvey = new Survey();
            FormChildProperties _FormData = new FormChildProperties()
            {
                //RecStatus = Status,
                FormId = ProjectMetaData.FormId,
                GlobalRecordID = ResponseId,
                RelateParentId = "GetFromParm",
                IsRelatedView = ProjectMetaData.IsRelatedView,
                FirstSaveTime = DateTime.UtcNow,
                LastSaveTime = DateTime.UtcNow,
                UserId = UserId.ToString()
            };
            _storeSurvey.FormChildProperties = new FormChildProperties();
            _storeSurvey.FormChildProperties = _FormData;
            var DocumentDBResponse = _surveyResponse.SaveSurveyQuestionInDocumentDBAsync(_storeSurvey);

            //  bool response = DocumentDBResponse.Result;
            return true;
        }

        #endregion


    }
}