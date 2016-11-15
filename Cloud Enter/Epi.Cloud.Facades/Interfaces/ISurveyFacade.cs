
using System.Collections.Generic;
using Epi.Web.Enter.Common.DTO;
using Epi.Web.Enter.Common.Message;
using Epi.Web.Enter.Common.Model;
using Epi.Web.MVC.Models;

namespace Epi.Cloud.Facades.Interfaces
{
    public interface ISurveyFacade
    {

        MvcDynamicForms.Form GetSurveyFormData(string surveyId, int pageNumber, SurveyAnswerDTO surveyAnswerDTO, bool isMobileDevice,
            List<SurveyAnswerDTO> surveyAnswerDTOList = null,
            List<FormsHierarchyDTO> formsHierarchyDTOList = null,
			bool IsAndroid = false);
        SurveyAnswerDTO CreateSurveyAnswer(string surveyId, string responseId, int userId, bool isChild = false, string relateResponseId = "", bool isEditMode = false, int currentOrgId = -1);
        SurveyAnswerResponse SaveSurveyAnswer(SurveyAnswerRequest surveyAnswerRequest);
        void UpdateSurveyResponse(SurveyInfoModel surveyInfoModel, string responseId, MvcDynamicForms.Form form, SurveyAnswerDTO surveyAnswerDTO, bool isSubmited, bool isSaved, int pageNumber, int userId);

        SurveyInfoModel GetSurveyInfoModel(string surveyId);
        List<FormInfoModel> GetFormsInfoModelList(FormsInfoRequest formsInfoRequest);
        SurveyInfoResponse GetChildFormInfo(SurveyInfoRequest SurveyInfoRequest);
        SurveyAnswerResponse GetSurveyAnswerResponse(string responseId, string formId = "", int userId = 0);
        SurveyAnswerResponse GetSurveyAnswerState(string responseId, string formId = "", int userId = 0);
        FormSettingResponse GetFormSettings(FormSettingRequest formSettingRequest);
        SurveyAnswerResponse GetFormResponseList(SurveyAnswerRequest surveyAnswerRequest);
        SurveyAnswerResponse SetChildRecord(SurveyAnswerRequest surveyAnswerRequest);
        FormSettingResponse SaveSettings(FormSettingRequest formSettingReq);
        SurveyAnswerResponse GetSurveyAnswerHierarchy(SurveyAnswerRequest surveyAnswerRequest);
        SurveyAnswerResponse GetResponsesByRelatedFormId(SurveyAnswerRequest surveyAnswerRequest);
        SurveyAnswerResponse DeleteResponse(SurveyAnswerRequest surveyAnswerRequest);
        void UpdateResponseStatus(SurveyAnswerRequest surveyAnswerRequest);
        bool HasResponse(string childFormId, string parentResponseId);
        FormsHierarchyResponse GetFormsHierarchy(FormsHierarchyRequest formsHierarchyRequest);
    }
}
