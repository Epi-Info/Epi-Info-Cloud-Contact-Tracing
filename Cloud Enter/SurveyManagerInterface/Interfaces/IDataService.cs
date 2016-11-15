using Epi.Web.Enter.Common.Message;

namespace Epi.Web.WCF.SurveyService
{
    public interface IDataService
    {
        SurveyInfoResponse GetSurveyInfo(SurveyInfoRequest pRequest);
        SurveyAnswerResponse GetSurveyAnswer(SurveyAnswerRequest pRequest);

        SurveyAnswerResponse SetSurveyAnswer(SurveyAnswerRequest pRequest);

        UserAuthenticationResponse PassCodeLogin(UserAuthenticationRequest pRequest);

		UserAuthenticationResponse SetPassCode(UserAuthenticationRequest pRequest);

		UserAuthenticationResponse GetAuthenticationResponse(UserAuthenticationRequest pRequest);

		FormsInfoResponse GetFormsInfo(FormsInfoRequest pRequest);

		FormResponseInfoResponse GetFormResponseInfo(FormResponseInfoRequest pRequest);

        SurveyAnswerResponse GetFormResponseList(SurveyAnswerRequest pRequest);

        FormSettingResponse GetFormSettings(FormSettingRequest pRequest);

        SurveyAnswerResponse DeleteResponse(SurveyAnswerRequest pRequest);

        UserAuthenticationResponse UserLogin(UserAuthenticationRequest pRequest);

		UserAuthenticationResponse GetUser(UserAuthenticationRequest request);

        bool UpdateUser(UserAuthenticationRequest request);

		FormSettingResponse SaveSettings(FormSettingRequest FormSettingReq);

		SurveyInfoResponse GetFormChildInfo(SurveyInfoRequest SurveyInfoRequest);

		FormsHierarchyResponse GetFormsHierarchy(FormsHierarchyRequest FormsHierarchyRequest);

		SurveyAnswerResponse GetSurveyAnswerHierarchy(SurveyAnswerRequest pRequest);

		SurveyAnswerResponse GetAncestorResponseIdsByChildId(SurveyAnswerRequest pRequest);

		SurveyAnswerResponse GetResponsesByRelatedFormId(SurveyAnswerRequest pRequest);

		OrganizationResponse GetOrganizationsByUserId(OrganizationRequest OrgRequest);

		OrganizationResponse GetUserOrganizations(OrganizationRequest OrgRequest);

		OrganizationResponse GetAdminOrganizations(OrganizationRequest OrgRequest);

		OrganizationResponse GetOrganizationInfo(OrganizationRequest OrgRequest);

		OrganizationResponse SetOrganization(OrganizationRequest request);

		OrganizationResponse GetOrganizationUsers(OrganizationRequest request);

		UserResponse GetUserInfo(UserRequest request);

		UserResponse SetUserInfo(UserRequest request);

		void UpdateResponseStatus(SurveyAnswerRequest Request);

		bool HasResponse(string SurveyId, string ResponseId);
    }

}
