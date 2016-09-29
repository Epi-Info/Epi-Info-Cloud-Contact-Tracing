using System;
using System.Collections.Generic;
using System.ServiceModel;
using Epi.Web.Enter.Common.Exception;
using Epi.Web.Enter.Common.Message;
using Epi.Web.MVC.Repositories.Core;
using Epi.Cloud.Common.Metadata;
using Epi.Cloud.DataEntryServices.Facade;
using Epi.Cloud.DataEntryServices.Model;
using Epi.Cloud.Interfaces.MetadataInterfaces;
using Epi.FormMetadata.DataStructures;
using static Epi.PersistenceServices.DocumentDB.DataStructures;

namespace Epi.Web.MVC.Repositories
{
    public class IntegratedSurveyAnswerRepository : RepositoryBase, ISurveyAnswerRepository
    {
        private Epi.Web.WCF.SurveyService.IEWEDataService _iDataService;
        private Epi.Cloud.Interfaces.DataInterface.IDataEntryService _dataEntryService;
        private ISurveyPersistenceFacade _isurveyDocumentDBStoreFacade;
        public IntegratedSurveyAnswerRepository(Epi.Web.WCF.SurveyService.IEWEDataService iDataService,
                                                Epi.Cloud.Interfaces.DataInterface.IDataEntryService dataEntryService,
                                                ISurveyPersistenceFacade isurveyDocumentDBStoreFacade)
        {
            _iDataService = iDataService;
            _dataEntryService = dataEntryService;
            _isurveyDocumentDBStoreFacade = isurveyDocumentDBStoreFacade;

        }

        /// <summary>
        /// Calling the proxy client to fetch a SurveyResponseResponse object
        /// </summary>
        /// <param name="surveyid"></param>
        /// <returns></returns>
        public SurveyAnswerResponse GetSurveyAnswer(SurveyAnswerRequest pRequest)
        {
            try
            {
                SurveyAnswerResponse result = _dataEntryService.GetSurveyAnswer(pRequest);
                return result;
            }
            catch (FaultException<CustomFaultException> cfe)
            {
                throw cfe;
            }
            catch (FaultException fe)
            {
                throw fe;
            }
            catch (CommunicationException ce)
            {
                throw ce;
            }
            catch (TimeoutException te)
            {
                throw te;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Calling the proxy client to fetch a SurveyResponseState object
        /// </summary>
        /// <param name="surveyid"></param>
        /// <returns></returns>
        public SurveyAnswerResponse GetSurveyAnswerState(SurveyAnswerRequest pRequest)
        {
            try
            {
                SurveyAnswerResponse result = _dataEntryService.GetSurveyAnswerState(pRequest);
                return result;
            }
            catch (FaultException<CustomFaultException> cfe)
            {
                throw cfe;
            }
            catch (FaultException fe)
            {
                throw fe;
            }
            catch (CommunicationException ce)
            {
                throw ce;
            }
            catch (TimeoutException te)
            {
                throw te;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public SurveyAnswerResponse GetFormResponseList(SurveyAnswerRequest pRequest)
        {
            try
            {
                //SurveyResponseResponse result = Client.GetSurveyResponse(pRequest);
                SurveyAnswerResponse result = _dataEntryService.GetFormResponseList(pRequest);
                return result;
            }
            catch (FaultException<CustomFaultException> cfe)
            {
                throw cfe;
            }
            catch (FaultException fe)
            {
                throw fe;
            }
            catch (CommunicationException ce)
            {
                throw ce;
            }
            catch (TimeoutException te)
            {
                throw te;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public FormSettingResponse GetFormSettings(FormSettingRequest pRequest)
        {
            try
            {

                FormSettingResponse result = _iDataService.GetFormSettings(pRequest);
                return result;
            }
            catch (FaultException<CustomFaultException> cfe)
            {
                throw cfe;
            }
            catch (FaultException fe)
            {
                throw fe;
            }
            catch (CommunicationException ce)
            {
                throw ce;
            }
            catch (TimeoutException te)
            {
                throw te;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public UserAuthenticationResponse UpdatePassCode(UserAuthenticationRequest AuthenticationRequest)
        {
            try
            {

                UserAuthenticationResponse result = _iDataService.SetPassCode(AuthenticationRequest);
                return result;
            }
            catch (FaultException<CustomFaultException> cfe)
            {
                throw cfe;
            }
            catch (FaultException fe)
            {
                throw fe;
            }
            catch (CommunicationException ce)
            {
                throw ce;
            }
            catch (TimeoutException te)
            {
                throw te;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public UserAuthenticationResponse ValidateUser(UserAuthenticationRequest pRequest)
        {
            try
            {

                //UserAuthenticationResponse result = _iDataService.PassCodeLogin(pRequest);
                UserAuthenticationResponse result = _iDataService.UserLogin(pRequest);
                return result;
            }
            catch (FaultException<CustomFaultException> cfe)
            {
                throw cfe;
            }
            catch (FaultException fe)
            {
                throw fe;
            }
            catch (CommunicationException ce)
            {
                throw ce;
            }
            catch (TimeoutException te)
            {
                throw te;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool UpdateUser(UserAuthenticationRequest pRequest)
        {
            try
            {

                //UserAuthenticationResponse result = _iDataService.PassCodeLogin(pRequest);
                return _iDataService.UpdateUser(pRequest);
            }
            catch (FaultException<CustomFaultException> cfe)
            {
                throw cfe;
            }
            catch (FaultException fe)
            {
                throw fe;
            }
            catch (CommunicationException ce)
            {
                throw ce;
            }
            catch (TimeoutException te)
            {
                throw te;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public UserAuthenticationResponse GetAuthenticationResponse(UserAuthenticationRequest pRequest)
        {
            try
            {

                UserAuthenticationResponse result = _iDataService.GetAuthenticationResponse(pRequest);
                return result;
            }
            catch (FaultException<CustomFaultException> cfe)
            {
                throw cfe;
            }
            catch (FaultException fe)
            {
                throw fe;
            }
            catch (CommunicationException ce)
            {
                throw ce;
            }
            catch (TimeoutException te)
            {
                throw te;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public PageResponseProperties SaveSurveyAnswer(SurveyAnswerRequest pRequest)
        {
            try
            {
                PageDigest[] pageDigests = (new MetadataAccessor(pRequest.Criteria.SurveyId)).GetCurrentFormPageDigests();

                //var results = _isurveyDocumentDBStoreFacade.SaveSurveyAnswerToDocumentDB(pageDigests, 1, pRequest.Criteria.UserId, pRequest.SurveyAnswerList[0].ResponseId);

                //SurveyAnswerResponse result = _dataEntryService.SetSurveyAnswer(pRequest);
                ////var result2 = _dataEntryService.SetSurveyAnswer(pRequest);
                //return result;
                throw new NotImplementedException();
            }
            catch (FaultException<CustomFaultException> cfe)
            {
                throw cfe;
            }
            catch (FaultException fe)
            {
                throw fe;
            }
            catch (CommunicationException ce)
            {
                throw ce;
            }
            catch (TimeoutException te)
            {
                throw te;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public SurveyAnswerResponse DeleteResponseXml(SurveyAnswerRequest pRequest)
        {
            try
            {
                SurveyAnswerResponse result = _dataEntryService.SetSurveyAnswer(pRequest);
                return result;
            }
            catch (FaultException<CustomFaultException> cfe)
            {
                throw cfe;
            }
            catch (FaultException fe)
            {
                throw fe;
            }
            catch (CommunicationException ce)
            {
                throw ce;
            }
            catch (TimeoutException te)
            {
                throw te;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public UserAuthenticationResponse GetUserInfo(UserAuthenticationRequest pRequest)
        {

            try
            {

                UserAuthenticationResponse result = _iDataService.GetUser(pRequest);
                return result;
            }
            catch (FaultException<CustomFaultException> cfe)
            {
                throw cfe;
            }
            catch (FaultException fe)
            {
                throw fe;
            }
            catch (CommunicationException ce)
            {
                throw ce;
            }
            catch (TimeoutException te)
            {
                throw te;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region stubcode
        public List<Enter.Common.DTO.SurveyAnswerDTO> GetList(Criterion criterion = null)
        {
            throw new NotImplementedException();
        }

        public Enter.Common.DTO.SurveyAnswerDTO Get(int id)
        {
            throw new NotImplementedException();
        }

        public int GetCount(Criterion criterion = null)
        {
            throw new NotImplementedException();
        }

        public void Insert(Enter.Common.DTO.SurveyAnswerDTO t)
        {
            throw new NotImplementedException();
        }

        public void Update(Enter.Common.DTO.SurveyAnswerDTO t)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
        #endregion


        List<SurveyAnswerResponse> IRepository<SurveyAnswerResponse>.GetList(Criterion criterion = null)
        {
            throw new NotImplementedException();
        }

        SurveyAnswerResponse IRepository<SurveyAnswerResponse>.Get(int id)
        {
            throw new NotImplementedException();
        }

        public void Insert(SurveyAnswerResponse t)
        {
            throw new NotImplementedException();
        }

        public void Update(SurveyAnswerResponse t)
        {
            throw new NotImplementedException();
        }
        public SurveyAnswerResponse SetChildRecord(SurveyAnswerRequest SurveyAnswerRequest)
        {

            try
            {
                SurveyAnswerResponse result = _dataEntryService.SetSurveyAnswer(SurveyAnswerRequest);
                return result;
            }
            catch (FaultException<CustomFaultException> cfe)
            {
                throw cfe;
            }
            catch (FaultException fe)
            {
                throw fe;
            }
            catch (CommunicationException ce)
            {
                throw ce;
            }
            catch (TimeoutException te)
            {
                throw te;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public FormSettingResponse SaveSettings(FormSettingRequest FormSettingReq)
        {

            try
            {

                FormSettingResponse result = _iDataService.SaveSettings(FormSettingReq);
                return result;
            }
            catch (FaultException<CustomFaultException> cfe)
            {
                throw cfe;
            }
            catch (FaultException fe)
            {
                throw fe;
            }
            catch (CommunicationException ce)
            {
                throw ce;
            }
            catch (TimeoutException te)
            {
                throw te;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public SurveyAnswerResponse GetSurveyAnswerHierarchy(SurveyAnswerRequest pRequest)
        {

            try
            {

                SurveyAnswerResponse result = _dataEntryService.GetSurveyAnswerHierarchy(pRequest);
                return result;
            }
            catch (FaultException<CustomFaultException> cfe)
            {
                throw cfe;
            }
            catch (FaultException fe)
            {
                throw fe;
            }
            catch (CommunicationException ce)
            {
                throw ce;
            }
            catch (TimeoutException te)
            {
                throw te;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public SurveyAnswerResponse GetSurveyAnswerAncestor(SurveyAnswerRequest pRequest)
        {

            try
            {

                SurveyAnswerResponse result = _dataEntryService.GetAncestorResponseIdsByChildId(pRequest);
                return result;
            }
            catch (FaultException<CustomFaultException> cfe)
            {
                throw cfe;
            }
            catch (FaultException fe)
            {
                throw fe;
            }
            catch (CommunicationException ce)
            {
                throw ce;
            }
            catch (TimeoutException te)
            {
                throw te;
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }

        public SurveyAnswerResponse GetResponsesByRelatedFormId(SurveyAnswerRequest FormResponseReq)
        {

            try
            {

                SurveyAnswerResponse result = _dataEntryService.GetResponsesByRelatedFormId(FormResponseReq);
                return result;
            }
            catch (FaultException<CustomFaultException> cfe)
            {
                throw cfe;
            }
            catch (FaultException fe)
            {
                throw fe;
            }
            catch (CommunicationException ce)
            {
                throw ce;
            }
            catch (TimeoutException te)
            {
                throw te;
            }
            catch (Exception ex)
            {
                throw ex;

            }

        }

        public OrganizationResponse GetOrganizationsByUserId(OrganizationRequest OrgReq)
        {

            try
            {

                OrganizationResponse result = _iDataService.GetOrganizationsByUserId(OrgReq);
                return result;
            }
            catch (FaultException<CustomFaultException> cfe)
            {
                throw cfe;
            }
            catch (FaultException fe)
            {
                throw fe;
            }
            catch (CommunicationException ce)
            {
                throw ce;
            }
            catch (TimeoutException te)
            {
                throw te;
            }
            catch (Exception ex)
            {
                throw ex;

            }

        }

        public OrganizationResponse GetUserOrganizations(OrganizationRequest OrgReq)
        {

            try
            {

                OrganizationResponse result = _iDataService.GetUserOrganizations(OrgReq);
                return result;
            }
            catch (FaultException<CustomFaultException> cfe)
            {
                throw cfe;
            }
            catch (FaultException fe)
            {
                throw fe;
            }
            catch (CommunicationException ce)
            {
                throw ce;
            }
            catch (TimeoutException te)
            {
                throw te;
            }
            catch (Exception ex)
            {
                throw ex;

            }

        }

        public OrganizationResponse GetAdminOrganizations(OrganizationRequest OrgReq)
        {

            try
            {

                OrganizationResponse result = _iDataService.GetAdminOrganizations(OrgReq);
                return result;
            }
            catch (FaultException<CustomFaultException> cfe)
            {
                throw cfe;
            }
            catch (FaultException fe)
            {
                throw fe;
            }
            catch (CommunicationException ce)
            {
                throw ce;
            }
            catch (TimeoutException te)
            {
                throw te;
            }
            catch (Exception ex)
            {
                throw ex;

            }

        }


        public OrganizationResponse GetOrganizationInfo(OrganizationRequest OrgRequest)
        {


            try
            {

                OrganizationResponse result = _iDataService.GetOrganizationInfo(OrgRequest);
                return result;
            }
            catch (FaultException<CustomFaultException> cfe)
            {
                throw cfe;
            }
            catch (FaultException fe)
            {
                throw fe;
            }
            catch (CommunicationException ce)
            {
                throw ce;
            }
            catch (TimeoutException te)
            {
                throw te;
            }
            catch (Exception ex)
            {
                throw ex;

            }



        }

        public OrganizationResponse SetOrganization(OrganizationRequest Request)
        {


            try
            {

                OrganizationResponse result = _iDataService.SetOrganization(Request);
                return result;
            }
            catch (FaultException<CustomFaultException> cfe)
            {
                throw cfe;
            }
            catch (FaultException fe)
            {
                throw fe;
            }
            catch (CommunicationException ce)
            {
                throw ce;
            }
            catch (TimeoutException te)
            {
                throw te;
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }

        public OrganizationResponse GetOrganizationUsers(OrganizationRequest OrgReq)
        {
            try
            {

                OrganizationResponse result = _iDataService.GetOrganizationUsers(OrgReq);
                return result;
            }
            catch (FaultException<CustomFaultException> cfe)
            {
                throw cfe;
            }
            catch (FaultException fe)
            {
                throw fe;
            }
            catch (CommunicationException ce)
            {
                throw ce;
            }
            catch (TimeoutException te)
            {
                throw te;
            }
            catch (Exception ex)
            {
                throw ex;

            }

        }
        public UserResponse GetUserInfo(UserRequest Request)
        {

            try
            {
                UserResponse result = _iDataService.GetUserInfo(Request);
                return result;
            }
            catch (FaultException<CustomFaultException> cfe)
            {
                throw cfe;
            }
            catch (FaultException fe)
            {
                throw fe;
            }
            catch (CommunicationException ce)
            {
                throw ce;
            }
            catch (TimeoutException te)
            {
                throw te;
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }

        public UserResponse SetUserInfo(UserRequest Request)
        {
            try
            {
                UserResponse result = _iDataService.SetUserInfo(Request);
                return result;
            }
            catch (FaultException<CustomFaultException> cfe)
            {
                throw cfe;
            }
            catch (FaultException fe)
            {
                throw fe;
            }
            catch (CommunicationException ce)
            {
                throw ce;
            }
            catch (TimeoutException te)
            {
                throw te;
            }
            catch (Exception ex)
            {
                throw ex;

            }


        }

        public void UpdateResponseStatus(SurveyAnswerRequest Request)
        {
            try
            {

                _dataEntryService.UpdateResponseStatus(Request);

            }
            catch (FaultException<CustomFaultException> cfe)
            {
                throw cfe;
            }
            catch (FaultException fe)
            {
                throw fe;
            }
            catch (CommunicationException ce)
            {
                throw ce;
            }
            catch (TimeoutException te)
            {
                throw te;
            }
            catch (Exception ex)
            {
                throw ex;

            }


        }

        public bool HasResponse(string SurveyId, string ResponseId)
        {
            try
            {
                return _dataEntryService.HasResponse(SurveyId, ResponseId);
            }
            catch (FaultException<CustomFaultException> cfe)
            {
                throw cfe;
            }
            catch (FaultException fe)
            {
                throw fe;
            }
            catch (CommunicationException ce)
            {
                throw ce;
            }
            catch (TimeoutException te)
            {
                throw te;
            }
            catch (Exception ex)
            {
                throw ex;

            }

        }

        SurveyAnswerResponse ISurveyAnswerRepository.SaveSurveyAnswer(SurveyAnswerRequest surveyAnswerRequest)
        {
            try
            {
                SurveyAnswerResponse result = _dataEntryService.SetSurveyAnswer(surveyAnswerRequest);
                return result;
            }
            catch (FaultException<CustomFaultException> cfe)
            {
                throw cfe;
            }
            catch (FaultException fe)
            {
                throw fe;
            }
            catch (CommunicationException ce)
            {
                throw ce;
            }
            catch (TimeoutException te)
            {
                throw te;
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }
    }
}