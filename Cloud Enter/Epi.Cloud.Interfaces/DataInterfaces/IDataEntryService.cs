using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.Message;
using Epi.Common.Core.Interfaces;

namespace Epi.Cloud.Interfaces.DataInterfaces
{
	public interface IDataEntryService
	{
		bool HasResponse(SurveyAnswerRequest surveyAnswerRequest);

		void UpdateResponseStatus(SurveyAnswerRequest surveyAnswerRequest);

		SurveyAnswerResponse GetSurveyAnswer(SurveyAnswerRequest surveyAnswerRequest);

		SurveyAnswerResponse GetSurveyAnswerState(SurveyAnswerRequest surveyAnswerRequest);

		SurveyAnswerResponse SetSurveyAnswer(SurveyAnswerRequest surveyAnswerRequest);

		SurveyAnswerResponse GetFormResponseList(SurveyAnswerRequest surveyAnswerRequest);

		SurveyAnswerResponse DeleteResponse(SurveyAnswerRequest surveyAnswerRequest);

		SurveyInfoResponse GetSurveyInfo(SurveyInfoRequest surveyInfoRequest);

		FormsInfoResponse GetFormsInfo(FormsInfoRequest formsInfoRequest);

		FormsHierarchyResponse GetFormsHierarchy(FormsHierarchyRequest formsHierarchyRequest);

		UserAuthenticationResponseBO GetAuthenticationResponse(UserAuthenticationRequestBO passcodeBO);
	}
}
