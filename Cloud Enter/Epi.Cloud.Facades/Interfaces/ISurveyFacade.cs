using System.Collections.Generic;
using Epi.Cloud.Common.DTO;
using Epi.Cloud.Common.Message;
using Epi.Cloud.Common.Model;
using Epi.Common.Core.Interfaces;
using Epi.Cloud.MVC.Models;

namespace Epi.Cloud.Facades.Interfaces
{
    public interface ISurveyFacade
    {
        MvcDynamicForms.Form GetSurveyFormData(string surveyId, int pageNumber, SurveyAnswerDTO surveyAnswerDTO, bool isMobileDevice,
            List<SurveyAnswerDTO> surveyAnswerDTOList = null,
            List<FormsHierarchyDTO> formsHierarchyDTOList = null,
			bool IsAndroid = false);

        SurveyAnswerDTO CreateSurveyAnswer(IResponseContext responseContext, bool isEditMode = false, int currentOrgId = -1);
        void UpdateSurveyResponse(SurveyInfoModel surveyInfoModel, string responseId, MvcDynamicForms.Form form, SurveyAnswerDTO surveyAnswerDTO, bool isSubmited, bool isSaved, int pageNumber, int orgId, int userId,string userName);

        SurveyInfoModel GetSurveyInfoModel(string surveyId);
        List<FormInfoModel> GetFormsInfoModelList(FormsInfoRequest formsInfoRequest);
        SurveyInfoResponse GetChildFormInfo(SurveyInfoRequest SurveyInfoRequest);
        FormSettingResponse GetFormSettings(FormSettingRequest formSettingRequest);
        List<FormSettingResponse> GetFormSettingsList(List<FormSettingRequest> formSettingRequest);
        SurveyAnswerResponse GetFormResponseList(SurveyAnswerRequest surveyAnswerRequest);
        SurveyAnswerResponse SetChildRecord(SurveyAnswerRequest surveyAnswerRequest);
        FormSettingResponse SaveSettings(FormSettingRequest formSettingReq);
        SurveyAnswerResponse DeleteResponse(SurveyAnswerRequest surveyAnswerRequest);
        void UpdateResponseStatus(SurveyAnswerRequest surveyAnswerRequest);
        FormsHierarchyResponse GetFormsHierarchy(FormsHierarchyRequest formsHierarchyRequest);
        bool HasResponse(SurveyAnswerRequest surveyAnswerRequest);
        SurveyAnswerDTO GetSurveyAnswerDTO(SurveyAnswerRequest surveyAnswerRequest);
        SurveyAnswerResponse GetSurveyAnswerState(SurveyAnswerRequest surveyAnswerRequest);
    }
}
