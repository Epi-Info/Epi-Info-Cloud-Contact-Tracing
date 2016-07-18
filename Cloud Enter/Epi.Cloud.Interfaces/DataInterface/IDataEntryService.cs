using System;
using Epi.Web.Enter.Common.Message;

namespace Epi.Cloud.Interfaces.DataInterface
{
    public interface IDataEntryService
    {
 
        bool HasResponse(string formId, string responseId);

        void UpdateResponseStatus(Epi.Web.Enter.Common.Message.SurveyAnswerRequest Request);

        SurveyAnswerResponse GetSurveyAnswer(SurveyAnswerRequest request);

        SurveyAnswerResponse SetSurveyAnswer(SurveyAnswerRequest request);

        SurveyAnswerResponse GetFormResponseList(SurveyAnswerRequest request);

        SurveyAnswerResponse GetResponsesByRelatedFormId(SurveyAnswerRequest request);

        SurveyAnswerResponse GetSurveyAnswerHierarchy(SurveyAnswerRequest request);

        SurveyAnswerResponse GetAncestorResponseIdsByChildId(SurveyAnswerRequest request);

        SurveyAnswerResponse DeleteResponse(SurveyAnswerRequest request);



        SurveyInfoResponse GetSurveyInfo(SurveyInfoRequest request);

        SurveyInfoResponse GetFormChildInfo(SurveyInfoRequest SurveyInfoRequest);



        FormsInfoResponse GetFormsInfo(FormsInfoRequest request);

        FormResponseInfoResponse GetFormResponseInfo(FormResponseInfoRequest request);

        FormsHierarchyResponse GetFormsHierarchy(FormsHierarchyRequest FormsHierarchyRequest);
    }
}
