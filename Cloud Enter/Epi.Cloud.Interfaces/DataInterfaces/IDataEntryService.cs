using Epi.Web.Enter.Common.BusinessObject;
using Epi.Web.Enter.Common.Message;

namespace Epi.Cloud.Interfaces.DataInterfaces
{
	public interface IDataEntryService
	{
 
		bool HasResponse(string childFormId, string parentResponseId);


		void UpdateResponseStatus(SurveyAnswerRequest surveyAnswerRequest);

		UserAuthenticationResponseBO GetAuthenticationResponse(UserAuthenticationRequestBO passcodeBO);


		SurveyAnswerResponse GetSurveyAnswer(SurveyAnswerRequest surveyAnswerRequest);

		SurveyAnswerResponse GetSurveyAnswerState(SurveyAnswerRequest surveyAnswerRequest);


		SurveyAnswerResponse SetSurveyAnswer(SurveyAnswerRequest surveyAnswerRequest);


		SurveyAnswerResponse GetFormResponseList(SurveyAnswerRequest surveyAnswerRequest);

		SurveyAnswerResponse GetResponsesByRelatedFormId(SurveyAnswerRequest surveyAnswerRequest);

		SurveyAnswerResponse GetSurveyAnswerHierarchy(SurveyAnswerRequest surveyAnswerRequest);

		SurveyAnswerResponse GetAncestorResponseIdsByChildId(SurveyAnswerRequest surveyAnswerRequest);

		SurveyAnswerResponse DeleteResponse(SurveyAnswerRequest surveyAnswerRequest);



		SurveyInfoResponse GetSurveyInfo(SurveyInfoRequest surveyInfoRequest);

		FormsInfoResponse GetFormsInfo(FormsInfoRequest formsInfoRequest);

		FormResponseInfoResponse GetFormResponseInfo(FormResponseInfoRequest formsInfoRequest);


		FormsHierarchyResponse GetFormsHierarchy(FormsHierarchyRequest formsHierarchyRequest);
	}
}
