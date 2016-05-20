﻿using System.Linq;
using Epi.Web.MVC.Repositories.Core;
using Epi.Web.Enter.Common.Message;
using Epi.Web.MVC.Utility;
using Epi.Web.MVC.Models;
using System.Collections.Generic;
using Epi.Web.Enter.Common.DTO;
namespace Epi.Web.MVC.Facade
{
    public class SurveyFacade : ISurveyFacade
    {

        // declare ISurveyInfoRepository which inherits IRepository of SurveyInfoResponse object
        private ISurveyInfoRepository _iSurveyInfoRepository;

        // declare ISurveyResponseRepository which inherits IRepository of SurveyResponseResponse object
        private ISurveyAnswerRepository _iSurveyAnswerRepository;
        public ISurveyAnswerRepository GetSurveyAnswerRepository() { return this._iSurveyAnswerRepository; }

        //declare SurveyInfoRequest
        private Epi.Web.Enter.Common.Message.SurveyInfoRequest _surveyInfoRequest;

        //declare SurveyResponseRequest
        private Epi.Web.Enter.Common.Message.SurveyAnswerRequest _surveyAnswerRequest;

        //declare SurveyAnswerDTO
        private Enter.Common.DTO.SurveyAnswerDTO _surveyAnswerDTO;

        //declare SurveyResponseXML object
        private SurveyResponseXML _surveyResponseXML;

        private FormInfoDTO _FormInfoDTO;

        /// <summary>
        /// Injectinting ISurveyInfoRepository through Constructor
        /// </summary>
        /// <param name="iSurveyInfoRepository"></param>
        public SurveyFacade(ISurveyInfoRepository iSurveyInfoRepository, ISurveyAnswerRepository iSurveyResponseRepository,
                                  Epi.Web.Enter.Common.Message.SurveyInfoRequest surveyInfoRequest, Epi.Web.Enter.Common.Message.SurveyAnswerRequest surveyResponseRequest,
                                  Enter.Common.DTO.SurveyAnswerDTO surveyAnswerDTO,
                                   SurveyResponseXML surveyResponseXML, UserAuthenticationRequest surveyAuthenticationRequest, FormInfoDTO FormInfoDTO)
        {
            _iSurveyInfoRepository = iSurveyInfoRepository;
            _iSurveyAnswerRepository = iSurveyResponseRepository;
            _surveyInfoRequest = surveyInfoRequest;
            _surveyAnswerRequest = surveyResponseRequest;
            _surveyAnswerDTO = surveyAnswerDTO;
            _surveyResponseXML = surveyResponseXML;
            _FormInfoDTO = FormInfoDTO;


        }

        /// <summary>
        /// get the survey form data
        /// </summary>
        /// <param name="surveyId"></param>
        /// <param name="responseId"></param>
        /// <param name="pageNumber"></param>
        /// <param name="surveyAnswerDTO"></param>
        /// <returns></returns>
        public MvcDynamicForms.Form GetSurveyFormData(
            string surveyId,
            int pageNumber,
            Epi.Web.Enter.Common.DTO.SurveyAnswerDTO surveyAnswerDTO,
            bool IsMobileDevice,
            List<SurveyAnswerDTO> _SurveyAnswerDTOList = null,
            List<Epi.Web.Enter.Common.DTO.FormsHierarchyDTO> FormsHierarchyDTOList = null)
        {
            List<SurveyInfoDTO> List = new List<SurveyInfoDTO>();


            //Get the SurveyInfoDTO
            Epi.Web.Enter.Common.DTO.SurveyInfoDTO surveyInfoDTO;
            if (FormsHierarchyDTOList == null)
            {
                surveyInfoDTO = SurveyHelper.GetSurveyInfoDTO(_surveyInfoRequest, _iSurveyInfoRepository, surveyId);
                if (_SurveyAnswerDTOList != null)
                {
                    foreach (var item in _SurveyAnswerDTOList)
                    {
                        Epi.Web.Enter.Common.Message.SurveyInfoRequest request = new SurveyInfoRequest();
                        request.Criteria.SurveyIdList.Add(item.SurveyId);
                        Epi.Web.Enter.Common.DTO.SurveyInfoDTO _SurveyInfoDTO = SurveyHelper.GetSurveyInfoDTO(request, _iSurveyInfoRepository, item.SurveyId);
                        List.Add(_SurveyInfoDTO);
                    }
                }
            }

            else
            {
                var SurveyInfoDTO = FormsHierarchyDTOList.First(x => x.FormId == surveyAnswerDTO.SurveyId);
                surveyInfoDTO = SurveyInfoDTO.SurveyInfo;

                _SurveyAnswerDTOList = new List<SurveyAnswerDTO>();
                _SurveyAnswerDTOList.Add(surveyAnswerDTO);

                foreach (var item in FormsHierarchyDTOList)
                {
                    if (item.ResponseIds.Count() > 0)
                    {
                        var DTO = item.ResponseIds.FirstOrDefault(z => z.ResponseId == surveyAnswerDTO.RelateParentId);
                        if (DTO != null && !_SurveyAnswerDTOList.Contains(DTO))

                            _SurveyAnswerDTOList.Add(DTO);

                    }
                }

                foreach (var item in _SurveyAnswerDTOList)
                {
                    var _SurveyInfoDTO = FormsHierarchyDTOList.FirstOrDefault(x => x.FormId == item.SurveyId);
                    List.Add(_SurveyInfoDTO.SurveyInfo);
                }


            }

            //TODO: Temporary Garry
            //System.IO.File.WriteAllText(@"C:\Temp\Template.XML", surveyInfoDTO.XML);

            MvcDynamicForms.Form form = null;

            if (IsMobileDevice)
            {
                Epi.Web.MVC.Utility.MobileFormProvider.SurveyInfoList = List;
                Epi.Web.MVC.Utility.MobileFormProvider.SurveyAnswerList = _SurveyAnswerDTOList;
                form = Epi.Web.MVC.Utility.MobileFormProvider.GetForm(surveyInfoDTO, pageNumber, surveyAnswerDTO);
            }
            else
            {
                Epi.Web.MVC.Utility.FormProvider.SurveyInfoList = List;
                Epi.Web.MVC.Utility.FormProvider.SurveyAnswerList = _SurveyAnswerDTOList;
                form = Epi.Web.MVC.Utility.FormProvider.GetForm(surveyInfoDTO, pageNumber, surveyAnswerDTO);
            }
            return form;
        }

        /// <summary>
        /// This method accepts a surveyId and responseId and creates the first survey response entry
        /// </summary>
        /// <param name="SurveyId"></param>
        /// <returns></returns>
        public Epi.Web.Enter.Common.DTO.SurveyAnswerDTO CreateSurveyAnswer(string surveyId, string responseId, int UserId, bool IsChild = false, string RelateResponseId = "", bool IsEditMode = false, int CurrentOrgId = -1)
        {

            return SurveyHelper.CreateSurveyResponse(surveyId, responseId, _surveyAnswerRequest, _surveyAnswerDTO, _surveyResponseXML, _iSurveyAnswerRepository, UserId, IsChild, RelateResponseId, IsEditMode, CurrentOrgId);
        }



        public void UpdateSurveyResponse(SurveyInfoModel surveyInfoModel, string responseId, MvcDynamicForms.Form form, Epi.Web.Enter.Common.DTO.SurveyAnswerDTO surveyAnswerDTO, bool IsSubmited, bool IsSaved, int PageNumber, int UserId)
        {
            // 1 Get the record for the current survey response
            // 2 update the current survey response and save the response

            //// 1 Get the record for the current survey response
            SurveyAnswerResponse surveyAnswerResponse = new SurveyAnswerResponse();//GetSurveyAnswerResponse(responseId, surveyInfoModel.SurveyId.ToString());
            surveyAnswerResponse.SurveyResponseList.Add(surveyAnswerDTO);
            ///2 Update the current survey response and save it

            SurveyHelper.UpdateSurveyResponse(surveyInfoModel, form, _surveyAnswerRequest, _surveyResponseXML, _iSurveyAnswerRepository, surveyAnswerResponse, responseId, surveyAnswerDTO, IsSubmited, IsSaved, PageNumber, UserId);
        }




        public SurveyInfoModel GetSurveyInfoModel(string surveyId)
        {
            _surveyInfoRequest.Criteria.SurveyIdList.Clear();
            _surveyInfoRequest.Criteria.SurveyIdList.Add(surveyId);
            SurveyInfoResponse surveyInfoResponse = _iSurveyInfoRepository.GetSurveyInfo(_surveyInfoRequest);
            SurveyInfoModel s = Mapper.ToSurveyInfoModel(surveyInfoResponse.SurveyInfoList[0]);
            return s;
        }



        /// <summary>
        /// Get the record for the current response (Step1: Saving Survey)
        /// </summary>
        /// <param name="ResponseId"></param>
        /// <returns></returns>
        public SurveyAnswerResponse GetSurveyAnswerResponse(string responseId, string FormId = "", int UserId = 0)
        {
            _surveyAnswerRequest.Criteria.SurveyAnswerIdList.Clear();
            _surveyAnswerRequest.Criteria.SurveyAnswerIdList.Add(responseId);
            _surveyAnswerRequest.Criteria.SurveyId = FormId;
            _surveyAnswerRequest.Criteria.UserId = UserId;
            SurveyAnswerResponse surveyAnswerResponse = _iSurveyAnswerRepository.GetSurveyAnswer(_surveyAnswerRequest);
            return surveyAnswerResponse;
        }


        /// <summary>
        /// Gets the information of Forms User has assigned/authorized.
        /// </summary>
        /// <param name="surveyId"></param>
        /// <returns></returns>

        public List<FormInfoModel> GetFormsInfoModelList(FormsInfoRequest formReq)
        {
            //_surveyInfoRequest.Criteria.SurveyIdList.Add(surveyId);
            //_surveyInfoRequest.Criteria.UserId = userId;
            //_surveyInfoRequest.Criteria.OrganizationKey = organizationKey;
            //FormsInfoRequest formReq = new FormsInfoRequest();



            FormsInfoResponse formInfoResponse = _iSurveyInfoRepository.GetFormsInfoList(formReq);

            //FormsInfoResponse GetSurveyInfoList(FormsInfoRequest pRequestId)
            List<FormInfoModel> listOfForms = new List<FormInfoModel>();

            foreach (var item in formInfoResponse.FormInfoList)
            {
                FormInfoModel formInfoModel = Mapper.ToFormInfoModel(item);
                listOfForms.Add(formInfoModel);
            }




            return listOfForms;
        }

        public SurveyAnswerResponse GetFormResponseList(SurveyAnswerRequest FormResponseReq)
        {

            SurveyAnswerResponse FormResponseList = _iSurveyAnswerRepository.GetFormResponseList(FormResponseReq);

            return FormResponseList;
        }
        public FormSettingResponse GetFormSettings(FormSettingRequest pRequest)
        {
            FormSettingResponse FormSettingResponse = _iSurveyAnswerRepository.GetFormSettings(pRequest);

            return FormSettingResponse;

        }


        public SurveyAnswerResponse DeleteResponse(SurveyAnswerRequest SARequest)
        {
            return _iSurveyInfoRepository.DeleteResponse(SARequest);
        }

        public SurveyAnswerResponse SetChildRecord(SurveyAnswerRequest SurveyAnswerRequest)
        {

            SurveyAnswerResponse SurveyAnswerResponse = _iSurveyAnswerRepository.SetChildRecord(SurveyAnswerRequest);

            return SurveyAnswerResponse;

        }

        public FormSettingResponse SaveSettings(FormSettingRequest FormSettingReq)
        {

            FormSettingResponse FormSettingResponse = _iSurveyAnswerRepository.SaveSettings(FormSettingReq);

            return FormSettingResponse;



        }

        public SurveyInfoResponse GetChildFormInfo(SurveyInfoRequest SurveyInfoRequest)
        {

            SurveyInfoResponse SurveyInfoResponse = _iSurveyInfoRepository.GetFormChildInfo(SurveyInfoRequest);
            return SurveyInfoResponse;
        }
        public FormsHierarchyResponse GetFormsHierarchy(FormsHierarchyRequest FormsHierarchyRequest)
        {


            FormsHierarchyResponse FormsHierarchyResponse = _iSurveyInfoRepository.GetFormsHierarchy(FormsHierarchyRequest);
            return FormsHierarchyResponse;

        }
        public SurveyAnswerResponse GetSurveyAnswerHierarchy(SurveyAnswerRequest pRequest)
        {

            SurveyAnswerResponse SurveyAnswerResponse = _iSurveyAnswerRepository.GetSurveyAnswerHierarchy(pRequest);

            return SurveyAnswerResponse;
        }

        public SurveyAnswerResponse GetAncestorResponses(SurveyAnswerRequest pRequest)
        {
            SurveyAnswerResponse SurveyAnswerResponse = _iSurveyAnswerRepository.GetSurveyAnswerAncestor(pRequest);

            return SurveyAnswerResponse;

        }
        public SurveyAnswerResponse GetResponsesByRelatedFormId(SurveyAnswerRequest FormResponseReq)
        {

            SurveyAnswerResponse SurveyAnswerResponse = _iSurveyAnswerRepository.GetResponsesByRelatedFormId(FormResponseReq);

            return SurveyAnswerResponse;



        }

        public void DeleteResponseXml(SurveyAnswerRequest FormResponseReq)
        {

            _iSurveyAnswerRepository.DeleteResponseXml(FormResponseReq);


        }
        public OrganizationResponse GetUserOrganizations(OrganizationRequest OrgRequest)
        {
            OrganizationResponse OrganizationResponse = new OrganizationResponse();
            OrganizationResponse = _iSurveyAnswerRepository.GetUserOrganizations(OrgRequest);

            return OrganizationResponse;


        }


        public OrganizationResponse GetOrganizationsByUserId(OrganizationRequest OrgRequest)
        {
            OrganizationResponse OrganizationResponse = new OrganizationResponse();
            OrganizationResponse = _iSurveyAnswerRepository.GetOrganizationsByUserId(OrgRequest);

            return OrganizationResponse;


        }

        public OrganizationResponse GetAdminOrganizations(OrganizationRequest OrgRequest)
        {
            OrganizationResponse OrganizationResponse = new OrganizationResponse();
            OrganizationResponse = _iSurveyAnswerRepository.GetAdminOrganizations(OrgRequest);

            return OrganizationResponse;


        }
        public OrganizationResponse GetOrganizationInfo(OrganizationRequest OrgRequest)
        {

            OrganizationResponse OrganizationResponse = new OrganizationResponse();
            OrganizationResponse = _iSurveyAnswerRepository.GetOrganizationInfo(OrgRequest);

            return OrganizationResponse;
        }
        public OrganizationResponse SetOrganization(OrganizationRequest Request)
        {

            OrganizationResponse OrganizationResponse = new OrganizationResponse();
            OrganizationResponse = _iSurveyAnswerRepository.SetOrganization(Request);

            return OrganizationResponse;

        }
        public OrganizationResponse GetOrganizationUsers(OrganizationRequest OrgRequest)
        {
            OrganizationResponse OrganizationResponse = new OrganizationResponse();
            OrganizationResponse = _iSurveyAnswerRepository.GetOrganizationUsers(OrgRequest);

            return OrganizationResponse;


        }
        public UserResponse GetUserInfo(UserRequest Request)
        {
            UserResponse UserResponse = new UserResponse();
            UserResponse = _iSurveyAnswerRepository.GetUserInfo(Request);
            return UserResponse;
        }
        public UserResponse SetUserInfo(UserRequest Request)
        {
            UserResponse UserResponse = new UserResponse();
            UserResponse = _iSurveyAnswerRepository.SetUserInfo(Request);
            return UserResponse;
        }

        public void UpdateResponseStatus(SurveyAnswerRequest Request)
        {
            _iSurveyAnswerRepository.UpdateResponseStatus(Request);

        }
        public bool HasResponse(string SurveyId, string ResponseId)
        {

            return _iSurveyAnswerRepository.HasResponse(SurveyId, ResponseId);
        }
    }
}