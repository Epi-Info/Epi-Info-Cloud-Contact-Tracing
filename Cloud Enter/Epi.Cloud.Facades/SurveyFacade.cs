using System.Collections.Generic;
using System.Linq;
using Epi.Cloud.Common.DTO;
using Epi.Cloud.Common.Message;
using Epi.Cloud.Common.Metadata;
using Epi.Cloud.Common.Model;
using Epi.Cloud.Facades.Interfaces;
using Epi.Cloud.Interfaces.DataInterfaces;
using Epi.Cloud.Interfaces.MetadataInterfaces;
using Epi.Cloud.MVC.Extensions;
using Epi.Common.Core.Interfaces;
using Epi.Cloud.MVC.Models;
using Epi.Cloud.MVC.Utility;

namespace Epi.Cloud.Facades
{
    public class SurveyFacade : ISurveyFacade
    {
        private readonly IDataEntryService _dataEntryService;
		private readonly ISurveyInfoService _surveyInfoService;
		private readonly IFormSettingsService _formSettingsService;
		private readonly ISecurityFacade _securityFacade;

		private Epi.Cloud.Common.Message.SurveyInfoRequest _surveyInfoRequest;

        private Epi.Cloud.Common.Message.SurveyAnswerRequest _surveyAnswerRequest;

        private SurveyAnswerDTO _surveyAnswerDTO;

        private SurveyResponseBuilder _surveyResponseBuilder;
			

        private FormInfoDTO _formInfoDTO;

        private readonly IProjectMetadataProvider _projectMetadataProvider;

        public SurveyFacade(IDataEntryService dataEntryService,
							ISurveyInfoService surveyInfoService,
							IFormSettingsService formSettingsService,
							ISecurityFacade securityFacade,

                            SurveyResponseBuilder surveyResponseBuilder, 
                            IProjectMetadataProvider projectMetadataProvider,

                            Epi.Cloud.Common.Message.SurveyInfoRequest surveyInfoRequest,
                            Epi.Cloud.Common.Message.SurveyAnswerRequest surveyResponseRequest,
                            Epi.Cloud.Common.Message.UserAuthenticationRequest surveyAuthenticationRequest,
							 
                            SurveyAnswerDTO surveyAnswerDTO,
                            FormInfoDTO formInfoDTO)
        {
			_dataEntryService = dataEntryService;
            _surveyInfoService = surveyInfoService;
			_formSettingsService = formSettingsService;
			_securityFacade = securityFacade;
            _surveyResponseBuilder = surveyResponseBuilder;
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
                        Epi.Cloud.Common.Message.SurveyInfoRequest request = new SurveyInfoRequest();
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
                        var DTO = item.ResponseIds.FirstOrDefault(z => z.ResponseId == surveyAnswerDTO.ParentResponseId);
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
        /// <param name="responseContext"></param>
        /// <param name="isEditMode"></param>
        /// <param name="currentOrgId"></param>
        /// <returns></returns>
        public SurveyAnswerDTO CreateSurveyAnswer(IResponseContext responseContext)
        {
            return SurveyHelper.CreateSurveyResponse(responseContext,
													 _surveyAnswerRequest, 
													 _surveyAnswerDTO, 
													 _surveyResponseBuilder, 
													 _dataEntryService);
        }

        public void UpdateSurveyResponse(SurveyInfoModel surveyInfoModel, 
                                         string responseId, 
                                         MvcDynamicForms.Form form, 
                                         SurveyAnswerDTO surveyAnswerDTO, 
                                         bool isSubmited, 
                                         bool isSaved, 
                                         int pageNumber, 
                                         int orgId,
                                         int userId, 
                                         string userName)
        {
            // 1 Get the record for the current survey response
            // 2 update the current survey response and save the response

            //// 1 Get the record for the current survey response
            SurveyAnswerResponse surveyAnswerResponse = new SurveyAnswerResponse();
            surveyAnswerResponse.SurveyResponseList.Add(surveyAnswerDTO);
            ///2 Update the current survey response and save it
            _surveyAnswerRequest.Criteria.UserId = userId;
            _surveyAnswerRequest.Criteria.UserName = userName;
            SurveyHelper.UpdateSurveyResponse(surveyInfoModel, 
                                              form, 
                                              _surveyAnswerRequest, 
                                              _surveyResponseBuilder, 
                                              _dataEntryService, 
                                              surveyAnswerResponse, 
                                              responseId, 
                                              surveyAnswerDTO, 
                                              isSubmited, 
                                              isSaved, 
                                              pageNumber, 
                                              orgId,
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
        public SurveyAnswerDTO GetSurveyAnswerDTO(SurveyAnswerRequest surveyAnswerRequest)
        {
            SurveyAnswerResponse surveyAnswerResponse = _dataEntryService.GetSurveyAnswer(surveyAnswerRequest);
            return surveyAnswerResponse.SurveyResponseList[0];
        }

        public SurveyAnswerResponse GetSurveyAnswerState(SurveyAnswerRequest surveyAnswerRequest)
        {
            SurveyAnswerResponse surveyAnswerResponse = _dataEntryService.GetSurveyAnswerState(surveyAnswerRequest);
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

        public List<FormSettingResponse> GetFormSettingsList(List<FormSettingRequest> formSettingRequestList)
        {
            List<FormSettingResponse> formSettingResponseList = _formSettingsService.GetFormSettingsList(formSettingRequestList);
            foreach (var formSettingResponse in formSettingResponseList)
            {
                AddColumnDigest(formSettingResponse);
            }
            return formSettingResponseList;
        }

        public FormSettingResponse GetFormSettings(FormSettingRequest formSettingRequest)
        {
            FormSettingResponse formSettingResponse = _formSettingsService.GetFormSettings(formSettingRequest);
            return AddColumnDigest(formSettingResponse);
        }

        private static FormSettingResponse AddColumnDigest(FormSettingResponse formSettingResponse)
        {
            var formId = formSettingResponse.FormInfo.FormId;
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

        public FormSettingResponse SaveSettings(FormSettingRequest formSettingRequest)
        {
            FormSettingResponse formSettingResponse = _formSettingsService.SaveSettings(formSettingRequest);

            return formSettingResponse;
        }

        public SurveyInfoResponse GetChildFormInfo(SurveyInfoRequest surveyInfoRequest)
        {

            SurveyInfoResponse surveyInfoResponse = _surveyInfoService.GetFormChildInfo(surveyInfoRequest);
            return surveyInfoResponse;
        }

        public void UpdateResponseStatus(SurveyAnswerRequest surveyAnswerRequest)
        {
            _dataEntryService.UpdateResponseStatus(surveyAnswerRequest);

        }

        public bool HasResponse(SurveyAnswerRequest surveyAnswerRequest)
        {
            return _dataEntryService.HasResponse(surveyAnswerRequest);
        }

		public FormsHierarchyResponse GetFormsHierarchy(FormsHierarchyRequest formsHierarchyRequest)
		{
			FormsHierarchyResponse FormsHierarchyResponse = _dataEntryService.GetFormsHierarchy(formsHierarchyRequest);
			return FormsHierarchyResponse;
		}
    }
}