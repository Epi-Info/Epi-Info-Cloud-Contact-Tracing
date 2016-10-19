using Epi.Web.MVC.Repositories.Core;
using Epi.Web.Enter.Common.Message;
using Epi.Web.MVC.Models;
using System.Collections.Generic;
using Epi.Web.Enter.Common.DTO;
using Epi.Cloud.DataEntryServices.Model;

namespace Epi.Web.MVC.Facade
{
    public interface ISurveyFacade
    {

        MvcDynamicForms.Form GetSurveyFormData(string surveyId, int pageNumber, SurveyAnswerDTO surveyAnswerDTO, bool IsMobileDevice,
            List<SurveyAnswerDTO> _SurveyAnswerDTOList = null,
            List<FormsHierarchyDTO> FormsHierarchyDTOList = null,
			bool IsAndroid = false);
        SurveyAnswerDTO CreateSurveyAnswer(string surveyId, string responseId, int UserId, bool IsChild = false, string RelateResponseId = "", bool IsEditMode = false, int CurrentOrgId = -1);
        void UpdateSurveyResponse(SurveyInfoModel surveyInfoModel, string responseId, MvcDynamicForms.Form form, SurveyAnswerDTO surveyAnswerDTO, bool IsSubmited, bool IsSaved, int PageNumber, int UserId);

        SurveyInfoModel GetSurveyInfoModel(string surveyId);
        List<FormInfoModel> GetFormsInfoModelList(FormsInfoRequest formReq);
        SurveyAnswerResponse GetSurveyAnswerResponse(string responseId, string formId = "", int userId = 0);
        SurveyAnswerResponse GetSurveyAnswerState(string responseId, string formId = "", int userId = 0);
        ISurveyAnswerRepository GetSurveyAnswerRepository();
        SurveyAnswerResponse GetFormResponseList(SurveyAnswerRequest FormResponseReq);
        FormSettingResponse GetFormSettings(FormSettingRequest FormSettingRequest);
        SurveyAnswerResponse SetChildRecord(SurveyAnswerRequest SurveyAnswerRequest);
        FormSettingResponse SaveSettings(FormSettingRequest FormSettingReq);
        SurveyInfoResponse GetChildFormInfo(SurveyInfoRequest SurveyInfoRequest);
        FormsHierarchyResponse GetFormsHierarchy(FormsHierarchyRequest FormsHierarchyRequest);
        SurveyAnswerResponse GetSurveyAnswerHierarchy(SurveyAnswerRequest pRequest);
        SurveyAnswerResponse GetAncestorResponses(SurveyAnswerRequest pRequest);
        SurveyAnswerResponse GetResponsesByRelatedFormId(SurveyAnswerRequest FormResponseReq);
        SurveyAnswerResponse DeleteResponse(SurveyAnswerRequest SARequest);
        void DeleteResponseNR(SurveyAnswerRequest FormResponseReq);
        OrganizationResponse GetOrganizationsByUserId(OrganizationRequest Request);
        OrganizationResponse GetUserOrganizations(OrganizationRequest Request);
        OrganizationResponse GetAdminOrganizations(OrganizationRequest Request);
        OrganizationResponse GetOrganizationInfo(OrganizationRequest Request);
        OrganizationResponse SetOrganization(OrganizationRequest Request);
        OrganizationResponse GetOrganizationUsers(OrganizationRequest OrgRequest);
        UserResponse GetUserInfo(UserRequest Request);
        UserResponse SetUserInfo(UserRequest Request);
        void UpdateResponseStatus(SurveyAnswerRequest Request);
        bool HasResponse(string SurveyId, string ResponseId);
    }
}
