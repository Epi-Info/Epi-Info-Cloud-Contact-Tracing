using System;
using System.Collections.Generic;
using System.Linq;
using Epi.Cloud.Common.Metadata;
using Epi.Cloud.Facades.Interfaces;
using Epi.Cloud.Interfaces.DataInterfaces;
using Epi.Cloud.Interfaces.MetadataInterfaces;
using Epi.Cloud.MVC.Extensions;
using Epi.Web.Enter.Common.DTO;
using Epi.Web.Enter.Common.Message;
using Epi.Web.Enter.Common.Model;
using Epi.Web.MVC.Models;
using Epi.Web.MVC.Utility;

namespace Epi.Cloud.Facades
{
    public class SurveyFacade : ISurveyFacade
    {

        private readonly IDataEntryService _dataEntryService;
		private readonly ISurveyInfoService _surveyInfoService;
		private readonly IFormSettingsService _formSettingsService;
		private readonly ISecurityFacade _securityFacade;

		private Epi.Web.Enter.Common.Message.SurveyInfoRequest _surveyInfoRequest;

        private Epi.Web.Enter.Common.Message.SurveyAnswerRequest _surveyAnswerRequest;

        private SurveyAnswerDTO _surveyAnswerDTO;

        private SurveyResponseHelper _surveyResponseHelper;
			

        private FormInfoDTO _formInfoDTO;

        private readonly IProjectMetadataProvider _projectMetadataProvider;

        public SurveyFacade(IDataEntryService dataEntryService,
							ISurveyInfoService surveyInfoService,
							IFormSettingsService formSettingsService,
							ISecurityFacade securityFacade,

                            SurveyResponseHelper surveyResponseXML, 
                            IProjectMetadataProvider projectMetadataProvider,

                            Epi.Web.Enter.Common.Message.SurveyInfoRequest surveyInfoRequest,
                            Epi.Web.Enter.Common.Message.SurveyAnswerRequest surveyResponseRequest,
                            Epi.Web.Enter.Common.Message.UserAuthenticationRequest surveyAuthenticationRequest,
							 
                            SurveyAnswerDTO surveyAnswerDTO,
                            FormInfoDTO formInfoDTO)
        {
			_dataEntryService = dataEntryService;
            _surveyInfoService = surveyInfoService;
			_formSettingsService = formSettingsService;
			_securityFacade = securityFacade;
            _surveyResponseHelper = surveyResponseXML;
            _projectMetadataProvider = projectMetadataProvider;

            _surveyInfoRequest = surveyInfoRequest;
            _surveyAnswerRequest = surveyResponseRequest;
            _surveyAnswerDTO = surveyAnswerDTO;
            _formInfoDTO = formInfoDTO;
        }

		/// <summary>
		/// get the survey form data
		/// </summary>
		/// <param name="surveyId"></param>
		/// <param name="pageNumber"></param>
		/// <param name="surveyAnswerDTO"></param>
		/// <param name="isMobileDevice"></param>
		/// <param name="surveyAnswerDTOList"></param>
		/// <param name="formsHierarchyDTOList"></param>
		/// <param name="isAndroid"></param>
		/// <returns></returns>
		public MvcDynamicForms.Form GetSurveyFormData(
            string surveyId,
            int pageNumber,
            SurveyAnswerDTO surveyAnswerDTO,
            bool isMobileDevice,
            List<SurveyAnswerDTO> surveyAnswerDTOList = null,
            List<FormsHierarchyDTO> formsHierarchyDTOList = null,
			bool isAndroid = false)
        {
            List<SurveyInfoDTO> surveyInfoDTOList = new List<SurveyInfoDTO>();

            //Get the SurveyInfoDTO
            SurveyInfoDTO surveyInfoDTO;
            if (formsHierarchyDTOList == null)
            {
                surveyInfoDTO = SurveyHelper.GetSurveyInfoDTO(_surveyInfoRequest, _surveyInfoService, surveyId);

                if (surveyAnswerDTOList != null)
                {
                    foreach (var item in surveyAnswerDTOList)
                    {
                        Epi.Web.Enter.Common.Message.SurveyInfoRequest request = new SurveyInfoRequest();
                        request.Criteria.SurveyIdList.Add(item.SurveyId);
                        SurveyInfoDTO surveyInfoDTO2 = SurveyHelper.GetSurveyInfoDTO(request, _surveyInfoService, item.SurveyId);
                        surveyInfoDTOList.Add(surveyInfoDTO2);
                    }
                }
            }
            else
            {
                var SurveyInfoDTO = formsHierarchyDTOList.First(x => x.FormId == (surveyAnswerDTO != null ? surveyAnswerDTO.SurveyId : surveyId));
                surveyInfoDTO = SurveyInfoDTO.SurveyInfo;

                surveyAnswerDTOList = new List<SurveyAnswerDTO>();
                surveyAnswerDTOList.Add(surveyAnswerDTO);

                foreach (var item in formsHierarchyDTOList)
                {
                    if (item.ResponseIds.Count() > 0)
                    {
                        var DTO = item.ResponseIds.FirstOrDefault(z => z.ResponseId == surveyAnswerDTO.RelateParentId);
                        if (DTO != null && !surveyAnswerDTOList.Contains(DTO))

                            surveyAnswerDTOList.Add(DTO);

                    }
                }

                foreach (var item in surveyAnswerDTOList)
                {
                    if (item != null)
                    {
                        var formsHierarchyDTO = formsHierarchyDTOList.FirstOrDefault(x => x.FormId == item.SurveyId);
                        surveyInfoDTOList.Add(formsHierarchyDTO.SurveyInfo);
                    }
                }
            }

            var formProvider = isMobileDevice ? new MobileFormProvider(surveyId) : new FormProvider(surveyId);
            MvcDynamicForms.Form form = formProvider.GetForm(surveyInfoDTO, pageNumber, surveyAnswerDTO, surveyAnswerDTOList, surveyInfoDTOList, isAndroid);

            return form;
        }

        /// <summary>
        /// This method accepts a surveyId and responseId and creates the first survey response entry
        /// </summary>
        /// <param name="SurveyId"></param>
        /// <returns></returns>
        public SurveyAnswerDTO CreateSurveyAnswer(string surveyId,
                                                  string responseId, 
                                                  int UserId, 
                                                  bool IsChild = false, 
                                                  string RelateResponseId = "", 
                                                  bool IsEditMode = false,
												  int CurrentOrgId = -1)
        {
            return SurveyHelper.CreateSurveyResponse(surveyId,
													 responseId, 
													 _surveyAnswerRequest, 
													 _surveyAnswerDTO, 
													 _surveyResponseHelper, 
													 _dataEntryService, 
													 UserId, 
													 IsChild, 
													 RelateResponseId, 
													 IsEditMode, 
													 CurrentOrgId);
        }

        public SurveyAnswerResponse SaveSurveyAnswer(SurveyAnswerRequest surveyAnswerRequest)
        {
            try
            {
                SurveyAnswerResponse result = _dataEntryService.SetSurveyAnswer(surveyAnswerRequest);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }


        public void UpdateSurveyResponse(SurveyInfoModel surveyInfoModel, 
                                         string responseId, 
                                         MvcDynamicForms.Form form, 
                                         SurveyAnswerDTO surveyAnswerDTO, 
                                         bool isSubmited, 
                                         bool isSaved, 
                                         int pageNumber, 
                                         int userId)
        {
            // 1 Get the record for the current survey response
            // 2 update the current survey response and save the response

            //// 1 Get the record for the current survey response
            SurveyAnswerResponse surveyAnswerResponse = new SurveyAnswerResponse();//GetSurveyAnswerResponse(responseId, surveyInfoModel.SurveyId.ToString());
            surveyAnswerResponse.SurveyResponseList.Add(surveyAnswerDTO);
            ///2 Update the current survey response and save it

            SurveyHelper.UpdateSurveyResponse(surveyInfoModel, 
                                              form, 
                                              _surveyAnswerRequest, 
                                              _surveyResponseHelper, 
                                              _dataEntryService, 
                                              surveyAnswerResponse, 
                                              responseId, 
                                              surveyAnswerDTO, 
                                              isSubmited, 
                                              isSaved, 
                                              pageNumber, 
                                              userId);
        }

        public SurveyInfoModel GetSurveyInfoModel(string surveyId)
        {
            _surveyInfoRequest.Criteria.SurveyIdList.Clear();
            _surveyInfoRequest.Criteria.SurveyIdList.Add(surveyId);
            SurveyInfoResponse surveyInfoResponse = _dataEntryService.GetSurveyInfo(_surveyInfoRequest);
            SurveyInfoModel surveyInfoModel = surveyInfoResponse.SurveyInfoList[0].ToSurveyInfoModel();
            return surveyInfoModel;
        }

        /// <summary>
        /// Get the record for the current response (Step1: Saving Survey)
        /// </summary>
        /// <param name="ResponseId"></param>
        /// <returns></returns>
        public SurveyAnswerResponse GetSurveyAnswerResponse(string responseId, string formId = "", int userId = 0)
        {
            _surveyAnswerRequest.Criteria.SurveyAnswerIdList.Clear();
            _surveyAnswerRequest.Criteria.SurveyAnswerIdList.Add(responseId);
            _surveyAnswerRequest.Criteria.SurveyId = formId;
            _surveyAnswerRequest.Criteria.UserId = userId;
            SurveyAnswerResponse surveyAnswerResponse = _dataEntryService.GetSurveyAnswer(_surveyAnswerRequest);
            return surveyAnswerResponse;
        }

        public SurveyAnswerResponse GetSurveyAnswerState(string responseId, string formId = "", int userId = 0)
        {
            _surveyAnswerRequest.Criteria.SurveyAnswerIdList.Clear();
            _surveyAnswerRequest.Criteria.SurveyAnswerIdList.Add(responseId);
            _surveyAnswerRequest.Criteria.SurveyId = formId;
            _surveyAnswerRequest.Criteria.UserId = userId;
            SurveyAnswerResponse surveyAnswerResponse = _dataEntryService.GetSurveyAnswerState(_surveyAnswerRequest);
            return surveyAnswerResponse;
        }

        /// <summary>
        /// Gets the information of Forms User has assigned/authorized.
        /// </summary>
        /// <param name="surveyId"></param>
        /// <returns></returns>
        public List<FormInfoModel> GetFormsInfoModelList(FormsInfoRequest formsInfoRequest)
        {
			FormsInfoResponse formInfoResponse = _dataEntryService.GetFormsInfo(formsInfoRequest);


			List<FormInfoModel> listOfForms = new List<FormInfoModel>();

            foreach (var item in formInfoResponse.FormInfoList)
            {
                FormInfoModel formInfoModel = item.ToFormInfoModel();
                listOfForms.Add(formInfoModel);
            }

			return listOfForms;
        }

        public SurveyAnswerResponse GetFormResponseList(SurveyAnswerRequest surveyAnswerRequest)
        {
            SurveyAnswerResponse FormResponseList = _dataEntryService.GetFormResponseList(surveyAnswerRequest);

            return FormResponseList;
        }

        public FormSettingResponse GetFormSettings(FormSettingRequest formSettingRequest)
        {
            var projectId = formSettingRequest.ProjectId;
            var formId = formSettingRequest.FormInfo.FormId;
            FormSettingResponse formSettingResponse = _formSettingsService.GetFormSettings(formSettingRequest);
            var formSetting = formSettingResponse.FormSetting;
			var metadataAccessor = new MetadataAccessor();
			var fieldDigests = metadataAccessor.GetFieldDigestsByFieldNames(formId, formSetting.ColumnNameList.Values);
            var reverseDictionary = formSetting.ColumnNameList.Select(t => new { t.Key, t.Value }).ToDictionary(t => t.Value, t => t.Key);
            formSetting.ColumnDigestList = fieldDigests.Select(t => new { Key = reverseDictionary[t.TrueCaseFieldName], Digest = t }).ToDictionary(t => t.Key, t => t.Digest);

            return formSettingResponse;
        }

        public SurveyAnswerResponse DeleteResponse(SurveyAnswerRequest surveyAnswerRequest)
        {
			// TODO: !! Verify that we expect the response and not the attachment to be deleted. !!
            return _dataEntryService.DeleteResponse(surveyAnswerRequest);
        }

        public SurveyAnswerResponse SetChildRecord(SurveyAnswerRequest surveyAnswerRequest)
        {
            SurveyAnswerResponse SurveyAnswerResponse = _dataEntryService.SetSurveyAnswer(surveyAnswerRequest);

            return SurveyAnswerResponse;
        }

        public FormSettingResponse SaveSettings(FormSettingRequest FormSettingReq)
        {
            FormSettingResponse FormSettingResponse = _formSettingsService.SaveSettings(FormSettingReq);

            return FormSettingResponse;
        }

        public SurveyInfoResponse GetChildFormInfo(SurveyInfoRequest SurveyInfoRequest)
        {

            SurveyInfoResponse SurveyInfoResponse = _surveyInfoService.GetFormChildInfo(SurveyInfoRequest);
            return SurveyInfoResponse;
        }

        public SurveyAnswerResponse GetSurveyAnswerHierarchy(SurveyAnswerRequest pRequest)
        {
            SurveyAnswerResponse SurveyAnswerResponse = _dataEntryService.GetSurveyAnswerHierarchy(pRequest);

            return SurveyAnswerResponse;
        }

        public SurveyAnswerResponse GetResponsesByRelatedFormId(SurveyAnswerRequest FormResponseReq)
        {
            SurveyAnswerResponse SurveyAnswerResponse = _dataEntryService.GetResponsesByRelatedFormId(FormResponseReq);

            return SurveyAnswerResponse;
        }

        public void UpdateResponseStatus(SurveyAnswerRequest Request)
        {
            _dataEntryService.UpdateResponseStatus(Request);

        }
        public bool HasResponse(string childFormId, string parentReponseId)
        {

            return _dataEntryService.HasResponse(childFormId, parentReponseId);
        }

		public FormsHierarchyResponse GetFormsHierarchy(FormsHierarchyRequest FormsHierarchyRequest)
		{
			FormsHierarchyResponse FormsHierarchyResponse = _dataEntryService.GetFormsHierarchy(FormsHierarchyRequest);
			return FormsHierarchyResponse;
		}
	}
}