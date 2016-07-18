using System;
using Epi.Cloud.Interfaces.DataInterface;
using Epi.Web.Enter.Common.Message;

namespace Epi.Cloud.DataEntryServices
{
    public class DataEntryService : IDataEntryService
    {
        private readonly Epi.Web.WCF.SurveyService.IEWEDataService _eweDataService;

        public DataEntryService(Epi.Web.WCF.SurveyService.IEWEDataService eweDataService)
        {
            _eweDataService = eweDataService;
        }

        public void UpdateResponseStatus(SurveyAnswerRequest request)
        {
            throw new NotImplementedException();
        }

        public SurveyAnswerResponse DeleteResponse(SurveyAnswerRequest request)
        {
            throw new NotImplementedException();
        }

        public SurveyAnswerResponse GetAncestorResponseIdsByChildId(SurveyAnswerRequest request)
        {
            throw new NotImplementedException();
        }

        public SurveyInfoResponse GetFormChildInfo(SurveyInfoRequest SurveyInfoRequest)
        {
            throw new NotImplementedException();
        }

        public FormResponseInfoResponse GetFormResponseInfo(FormResponseInfoRequest request)
        {
            throw new NotImplementedException();
        }

        public SurveyAnswerResponse GetFormResponseList(SurveyAnswerRequest request)
        {
            var sar = _eweDataService.GetFormResponseList(request);
            return sar;
        }

        public FormSettingResponse GetFormSettings(FormSettingRequest request)
        {
            throw new NotImplementedException();
        }

        public FormsHierarchyResponse GetFormsHierarchy(FormsHierarchyRequest request)
        {
            throw new NotImplementedException();
        }

        public FormsInfoResponse GetFormsInfo(FormsInfoRequest request)
        {
            throw new NotImplementedException();
        }


        public SurveyAnswerResponse GetResponsesByRelatedFormId(SurveyAnswerRequest request)
        {
            throw new NotImplementedException();
        }

        public SurveyAnswerResponse GetSurveyAnswer(SurveyAnswerRequest request)
        {
            return new SurveyAnswerResponse();
        }

        public SurveyAnswerResponse GetSurveyAnswerHierarchy(SurveyAnswerRequest request)
        {
            throw new NotImplementedException();
        }

        public SurveyInfoResponse GetSurveyInfo(SurveyInfoRequest request)
        {
            throw new NotImplementedException();
        }

        public bool HasResponse(string SurveyId, string ResponseId)
        {
            throw new NotImplementedException();
        }

        public SurveyAnswerResponse SetSurveyAnswer(SurveyAnswerRequest request)
        {
            return new SurveyAnswerResponse();
        }


        #region Security Related
        public UserAuthenticationResponse PassCodeLogin(UserAuthenticationRequest request)
        {
            throw new NotImplementedException();
        }

        public FormSettingResponse SaveSettings(FormSettingRequest FormSettingReq)
        {
            throw new NotImplementedException();
        }

        public OrganizationResponse SetOrganization(OrganizationRequest request)
        {
            throw new NotImplementedException();
        }

        public UserAuthenticationResponse SetPassCode(UserAuthenticationRequest request)
        {
            throw new NotImplementedException();
        }

        public UserAuthenticationResponse GetUser(UserAuthenticationRequest request)
        {
            throw new NotImplementedException();
        }

        public UserResponse GetUserInfo(UserRequest request)
        {
            throw new NotImplementedException();
        }

        public UserResponse SetUserInfo(UserRequest request)
        {
            throw new NotImplementedException();
        }

        public OrganizationResponse GetUserOrganizations(OrganizationRequest OrgRequest)
        {
            throw new NotImplementedException();
        }

        public bool UpdateUser(UserAuthenticationRequest request)
        {
            throw new NotImplementedException();
        }

        public UserAuthenticationResponse UserLogin(UserAuthenticationRequest request)
        {
            throw new NotImplementedException();
        }
        public OrganizationResponse GetAdminOrganizations(OrganizationRequest OrgRequest)
        {
            throw new NotImplementedException();
        }

        public OrganizationResponse GetOrganizationInfo(OrganizationRequest OrgRequest)
        {
            throw new NotImplementedException();
        }

        public OrganizationResponse GetOrganizationsByUserId(OrganizationRequest OrgRequest)
        {
            throw new NotImplementedException();
        }

        public OrganizationResponse GetOrganizationUsers(OrganizationRequest request)
        {
            throw new NotImplementedException();
        }
        public UserAuthenticationResponse GetAuthenticationResponse(UserAuthenticationRequest request)
        {
            throw new NotImplementedException();
        }
        #endregion Security Related
    }
}
