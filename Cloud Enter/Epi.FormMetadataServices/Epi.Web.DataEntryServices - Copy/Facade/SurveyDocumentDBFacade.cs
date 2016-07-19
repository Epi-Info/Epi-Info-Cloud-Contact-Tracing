using System.Collections.Generic;
using Epi.Web.Enter.Common.DTO;
using MvcDynamicForms;
using Epi.Cloud.DataEntryServices.Model;
using System;
using System.Threading.Tasks;
using Epi.Web.Enter.Common.Message;
using Epi.Cloud.Common.Metadata;
using Epi.Cloud.Interfaces.MetadataInterfaces;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.DataEntryServices.Extensions;
using Epi.Cloud.Common.EntityObjects;

namespace Epi.Cloud.DataEntryServices.Facade
{
    public class SurveyDocumentDBFacade : MetadataAccessor, ISurveyStoreDocumentDBFacade
    {
        public SurveyDocumentDBFacade(IProjectMetadataProvider projectMetadataProvider)
        {
            ProjectMetadataProvider = projectMetadataProvider;
        }


        CRUDSurveyResponse _surveyResponse = new CRUDSurveyResponse();

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
            _storeSurvey.PageResponseDetail = ReadQuestionAndAnswerFromPage(form, _storeSurvey, surveyInfoModel, responseId);
            bool response = await _surveyResponse.InsertToSurveyToDocumentDBAsync(_storeSurvey);
            return true;
        }
        #endregion

        #region GetCollectionName
        public FormDocumentDBEntity GetDbAndCollectionName(Form form, string DbName)
        {
            FormDocumentDBEntity _survey = new FormDocumentDBEntity();
            var pageId = Convert.ToInt32(form.PageId);
            var pageDigest = form.SurveyInfo.GetCurrentFormPageDigestByPageId(pageId);
            _survey.SurveyName = pageDigest.FormName;
            _survey.IsChildForm = pageDigest.IsRelatedView;
            _survey.CollectionName = DbName + pageDigest.PageId;
            _survey.DatabaseName = DbName;
            return _survey;
        }
        #endregion

        #region Read Question and Answer from all pages
        public PageResponseDetailResource ReadQuestionAndAnswerFromPage(Form form, FormDocumentDBEntity _surveyInfo, SurveyInfoModel surveyInfoModel, string responseId)
        {
            PageResponseDetailResource _surveyQA = new PageResponseDetailResource();
            _surveyQA.ResponseQA = new Dictionary<string, string>();
            foreach (var field in form.InputFields)
            {
                if (!field.IsPlaceHolder)
                {
                    _surveyQA.ResponseQA.Add(field.Key, field.Response);
                }
            }
            _surveyQA.GlobalRecordID = responseId;
            return _surveyQA;
        }
        #endregion 

        

        #region ReadSurveyAnswerByResponseID,PageId 
        public PageResponseDetail ReadSurveyAnswerByResponseID(string surveyName, string surveyId, string responseId, string pageId)
        {
            PageResponseDetailResource pageResponseDetailResource = this._surveyResponse.ReadSurveyFromDocumentDBByPageandRespondIdAsync(surveyName, null, responseId, pageId);
            PageResponseDetail formResponseDetail = pageResponseDetailResource != null
                                                    ? pageResponseDetailResource.ToPageResponseDetail()
                                                    : new PageResponseDetail { GlobalRecordID = responseId, PageId = Convert.ToInt32(pageId) };
            return formResponseDetail;
        }

        #endregion

        #region DeleteSurveyByResponseId
        public SurveyAnswerResponse DeleteResponse(FormDocumentDBEntity SARequest)
        {
            SurveyAnswerResponse surveyAnsResponse = new SurveyAnswerResponse();
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


        public FormsHierarchyDTO GetChildRecordByChildFormId(string childFormId, string relateParentId, string dbName, Dictionary<int, FieldDigest> parameters)
        {
            FormsHierarchyDTO formhierarchyDTO = new FormsHierarchyDTO();
            var childRecords = _surveyResponse.ReadAllRecordByChildFormIdandRelateParentId(childFormId, relateParentId, dbName, parameters);
            formhierarchyDTO.ResponseIds = new List<SurveyAnswerDTO>();
            foreach (var item in childRecords)
            {
                SurveyAnswerDTO surveyAnswer = new SurveyAnswerDTO();
                surveyAnswer.ResponseId = item.ResponseId.ToString();
                surveyAnswer.SqlData = item.ResponseDetail.FlattenedResponseQA(key => key.ToLower());
                formhierarchyDTO.ResponseIds.Add(surveyAnswer);
            }
          
            return formhierarchyDTO;
        }

        public bool SaveFormPropertiesToDocumentDB(SurveyAnswerRequest request)
        {

            FormDocumentDBEntity storeForm = new FormDocumentDBEntity(); 
            if(request.Criteria.IsDeleteMode)
            {
                request.SurveyAnswerList[0].Status = RecordStatus.Deleted;
            } 
            FormPropertiesResource formData = new FormPropertiesResource()
            {
                RecStatus = request.SurveyAnswerList[0].Status,
                FormId = request.Criteria.SurveyId,
                FormName = request.Criteria.FormName,
                GlobalRecordID = request.SurveyAnswerList[0].ResponseId,
                RelateParentId = request.SurveyAnswerList[0].RelateParentId,
                //IsRelatedView = ProjectMetaData.IsRelatedView,
                FirstSaveTime = DateTime.UtcNow,
                LastSaveTime = DateTime.UtcNow,
                UserId = request.Criteria.UserId.ToString()
            };

            storeForm.FormProperties = new FormPropertiesResource();
            storeForm.FormProperties = formData;
            storeForm.GlobalRecordID = request.RequestId; 
            var saveTask = _surveyResponse.SaveSurveyQuestionInDocumentDBAsync(storeForm);
            return true;
        }

        #endregion

        #region Open the Record by FormId and GlobalRecordID
        public SurveyAnswerResponse GetSurveyAnswerResponse(string surveyName,string responseId, string formId, int userId,string collectionName)
        { 
            SurveyAnswerResponse _surveyAnswerResponse = new SurveyAnswerResponse();
            _surveyAnswerResponse.SurveyResponseList = new List<SurveyAnswerDTO>();

            //Check GlobalRecordIs isDelete or Not
            var result = _surveyResponse.ReadFormInfoByGlobalRecordIdAndSurveyId(surveyName, formId, responseId, "1");
            List<SurveyAnswerDTO> SurveyResponseList = new List<SurveyAnswerDTO>();
            SurveyAnswerDTO _surveyAnswerDTO = new SurveyAnswerDTO();
            _surveyAnswerDTO.ResponseId = result.FormProperties.GlobalRecordID;
            _surveyAnswerDTO.SurveyId = formId;
            _surveyAnswerDTO.ResponseDetail = result.PageResponseDetail.ToFormResponseDetail(formId, result.FormProperties.FormName, result.FormProperties.RelateParentId);
            _surveyAnswerDTO.DateCreated = result.FormProperties.FirstSaveTime;
            _surveyAnswerDTO.DateUpdated = result.FormProperties.LastSaveTime;
            _surveyAnswerDTO.RelateParentId = result.FormProperties.RelateParentId;
            _surveyAnswerDTO.Status = RecordStatus.Saved;// Result.FormProperties.RecStatus;
            SurveyResponseList.Add(_surveyAnswerDTO); 
            _surveyAnswerResponse.SurveyResponseList = SurveyResponseList;
            return _surveyAnswerResponse;
        }

        public IEnumerable<SurveyResponse> ReadAllRecordsBySurveyID(string formName, IDictionary<int, FieldDigest> gridFieldDigests)
        {
            return _surveyResponse.ReadAllRecordsBySurveyID(formName, gridFieldDigests);
        }
        #endregion
    }
}