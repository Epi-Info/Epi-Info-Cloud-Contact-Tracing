using System.Collections.Generic;
using Epi.Web.Enter.Common.DTO;
using MvcDynamicForms;
using Epi.Cloud.DataEntryServices.Model;
using System;
using System.Threading.Tasks;
using Epi.Web.Enter.Common.Message;
using Epi.Cloud.Common.Metadata;
using Epi.Cloud.DataEntryServices.Helpers;
using Newtonsoft.Json;
using Epi.Cloud.ServiceBus;

namespace Epi.Cloud.DataEntryServices.Facade
{
    public class SurveyDocumentDBFacade : ISurveyStoreDocumentDBFacade
    {


        CRUDSurveyResponse _crudSurveyResponse = new CRUDSurveyResponse();

        /// <summary>
        /// Insert survey question and answer to Document Db
        /// </summary>

        #region Insert Into Servey Response to DocumentDB
        public async Task<bool> InsertSurveyResponseToDocumentDBStoreAsync(SurveyInfoModel surveyInfoModel, string responseId, Form form, SurveyAnswerDTO surveyAnswerDTO, bool IsSubmited, bool IsSaved, int PageNumber, int UserId)
        { 
            FormDocumentDBEntity _storeSurvey = new FormDocumentDBEntity();
            string Dbname = surveyInfoModel.SurveyName;
            //Get Collection Name
            _storeSurvey = GetDbAndCollectionName(form, Dbname);
            _storeSurvey.FormQuestionandAnswer = ReadQuestionandAnswerFromPage(form, _storeSurvey, surveyInfoModel, responseId);
            bool response = await _crudSurveyResponse.InsertToSurveyToDocumentDBAsync(_storeSurvey);
            return true;
        }
        #endregion

        #region GetCollectionName
        public FormDocumentDBEntity GetDbAndCollectionName(Form form, string DbName)
        {
            FormDocumentDBEntity _survey = new FormDocumentDBEntity();
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
        public FormQuestionandAnswer ReadQuestionandAnswerFromPage(Form form, FormDocumentDBEntity _surveyInfo, SurveyInfoModel surveyInfoModel, string responseId)
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

        #region ReadSurveyAnswerByResponseID,PageId 
        public FormQuestionandAnswer ReadSurveyAnswerByResponseID(string surveyName, string surveyId, string responseId, string pageId)
        { 
            FormQuestionandAnswer formResponse = _crudSurveyResponse.ReadSurveyFromDocumentDBByPageandRespondIdAsync(surveyName,null, responseId, pageId);
            return formResponse;
        }

        #endregion

        #region DeleteSurveyByResponseId
        public SurveyAnswerResponse DeleteResponse(FormDocumentDBEntity SARequest)
        {
            SurveyAnswerResponse surveyAnsResponse = new SurveyAnswerResponse();
            CRUDSurveyResponse _surveyResponse = new CRUDSurveyResponse();
            //var tasks = _surveyResponse.DeleteDocumentByIdAsync(SARequest);
            //var result = tasks.Result;
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


        public FormsHierarchyDTO GetChildRecordByChildFormId(string ChildFormId, string RelateParentId, string DbName, Dictionary<int, FieldDigest> Params)
        { 
            FormsHierarchyDTO _formhierarchyDTO = new FormsHierarchyDTO();
            var GetChildRecords = _crudSurveyResponse.ReadAllRecordByChildFormIdandRelateParentId(ChildFormId, RelateParentId, DbName, Params);
            _formhierarchyDTO.ResponseIds = new List<SurveyAnswerDTO>();
            //SurveyAnswerDTO _surveyAnswer = new SurveyAnswerDTO();
            foreach (var item in GetChildRecords)
            {
                SurveyAnswerDTO _surveyAnswer = new SurveyAnswerDTO();
                _surveyAnswer.ResponseId = item.ResponseId.ToString();
                _surveyAnswer.SqlData = item.ResponseQA;
                _formhierarchyDTO.ResponseIds.Add(_surveyAnswer);
            }
          
            return _formhierarchyDTO;
        }

        public bool SaveFormPropertiesToDocumentDB(SurveyAnswerRequest request)
        {

            CRUDSurveyResponse _surveyResponse = new CRUDSurveyResponse();
            FormDocumentDBEntity _storeForm = new FormDocumentDBEntity(); 
            if(request.Criteria.IsDeleteMode)
            {
                request.SurveyAnswerList[0].Status = 0;
            }
            FormProperties _FormData = new FormProperties()
            {
                RecStatus = request.SurveyAnswerList[0].Status,
                FormId = request.Criteria.SurveyId,
                FormName = request.Criteria.FormName,
                GlobalRecordID = request.SurveyAnswerList[0].ResponseId,
                RelateParentId = request.SurveyAnswerList[0].RelateParentId,
                //IsRelatedView = ProjectMetaData.IsRelatedView,
                FirstSaveTime = DateTime.UtcNow,
                LastSaveTime = DateTime.UtcNow,
                UserId = request.Criteria.UserId,
                IsDraftMode = request.Criteria.IsDraftMode
            };

            _storeForm.FormProperties = new FormProperties();
            _storeForm.FormProperties = _FormData;
            _storeForm.GlobalRecordID = request.RequestId; 
            var DocumentDBResponse = _surveyResponse.SaveSurveyQuestionInDocumentDBAsync(_storeForm);
            //if(_FormData.RecStatus==2)
            //{
            //  //  var test = ReadFormInfo(_storeForm.GlobalRecordID);
            //    string Query= "Select * from FormInfo where FormInfo.GlobalRecordID ='"+_FormData.GlobalRecordID +"' and FormInfo.RecStatus!=0" ;
            //    var FormInfo =(FormProperties)_surveyResponse.ReadDataFromCollection(_FormData.GlobalRecordID, "FormInfo", Query);
            //    CURDServiceBus crudServicBus = new CURDServiceBus();
            //    if(FormInfo!=null)
            //    {
            //        //send form info to ServiceBus
            //        crudServicBus.SendMessagesToTopic(FormInfo.Id, JsonConvert.SerializeObject(FormInfo));
            //    }
               
            //}
            return true;
        }

        #endregion 

        #region Open the Record by FormId and GlobalRecordID
        public SurveyAnswerResponse GetSurveyAnswerResponse(string surveyName,string responseId, string FormId, int UserId,string collectionName)
        { 
            SurveyAnswerResponse _surveyAnswerResponse = new SurveyAnswerResponse();
            _surveyAnswerResponse.SurveyResponseList = new List<SurveyAnswerDTO>();

             
            var Result = _crudSurveyResponse.ReadFormInfoByGlobalRecordIdAndSurveyId(surveyName, FormId, responseId,"1").Result;
            List<SurveyAnswerDTO> SurveyResponseList = new List<SurveyAnswerDTO>();
            SurveyAnswerDTO _surveyAnswerDTO = new SurveyAnswerDTO();
            _surveyAnswerDTO.ResponseId = Result.FormProperties.GlobalRecordID;
            _surveyAnswerDTO.SurveyId = FormId;
            _surveyAnswerDTO.ResponseQA = Result.FormQuestionandAnswer.SurveyQAList;
            _surveyAnswerDTO.DateCreated = Result.FormProperties.FirstSaveTime;
            _surveyAnswerDTO.DateUpdated = Result.FormProperties.LastSaveTime;
            _surveyAnswerDTO.RelateParentId = Result.FormProperties.RelateParentId;
            _surveyAnswerDTO.Status = Result.FormProperties.RecStatus;
            _surveyAnswerDTO.LastActiveUserId =Result.FormProperties.UserId;
            SurveyResponseList.Add(_surveyAnswerDTO); 
            _surveyAnswerResponse.SurveyResponseList = SurveyResponseList;
            return _surveyAnswerResponse;
        }
        #endregion


        #region Read Forminfo for DataConsisitencyServiceAPI
        public List<string> ReadFormInfo(string globalID)
        { 
            var responseToDCSApi = _crudSurveyResponse.ReadfullFormInfo(globalID);
            return responseToDCSApi;
        }
        #endregion


    }
}