using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.DTO;
using Epi.Cloud.Common.Message;
using Epi.Common.Core.Interfaces;

namespace Epi.Cloud.Interfaces.DataInterfaces
{
	public interface IDataEntryService
	{
		bool HasResponse(SurveyAnswerRequest surveyAnswerRequest);

        SurveyAnswerResponse UpdateResponseStatus(SurveyAnswerRequest surveyAnswerRequest);

		SurveyAnswerStateDTO GetSurveyAnswerState(SurveyAnswerRequest surveyAnswerRequest);

		SurveyAnswerResponse GetSurveyAnswer(SurveyAnswerRequest surveyAnswerRequest);

		SurveyAnswerResponse SetSurveyAnswer(SurveyAnswerRequest surveyAnswerRequest);

		SurveyAnswerResponse GetFormResponseList(SurveyAnswerRequest surveyAnswerRequest);

		SurveyAnswerResponse DeleteResponse(SurveyAnswerRequest surveyAnswerRequest);

		SurveyInfoResponse GetSurveyInfo(SurveyInfoRequest surveyInfoRequest);

		FormsInfoResponse GetFormsInfo(FormsInfoRequest formsInfoRequest);

		FormsHierarchyResponse GetFormsHierarchy(FormsHierarchyRequest formsHierarchyRequest);

		UserAuthenticationResponseBO GetAuthenticationResponse(UserAuthenticationRequestBO passcodeBO);
	}
}
