using System.Collections.Generic;
using System.Threading.Tasks;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.DataEntryServices.Model;
using Epi.DataPersistence.DataStructures;
using Epi.FormMetadata.DataStructures;
using Epi.Web.Enter.Common.BusinessObject;
using Epi.Web.Enter.Common.DTO;
using Epi.Web.Enter.Common.Message;

namespace Epi.Cloud.DataEntryServices.Interfaces
{
	public interface ISurveyPersistenceFacade
    {
        bool DoChildrenExistForResponseId(string responseId);

        bool UpdateResponseStatus(string responseId, int recordStatus, RecordStatusChangeReason reasonForStatusChange);

		int GetFormResponseCount(string formId, bool includeDeletedRecords = false);

        FormResponseDetail GetFormResponseState(string responseId);

        FormResponseDetail GetFormResponseByResponseId(string responseId);

		Task<bool> InsertResponse(SurveyResponseBO surveyResponseBO);

		bool InsertResponse(MvcDynamicForms.Form form, SurveyResponseBO surveyResponseBO);

        Task<bool> InsertChildResponseAsync(SurveyResponseBO surveyResponseBO);

        PageResponseDetail ReadSurveyAnswerByResponseID(string surveyId, string responseId, int pageId);

        SurveyAnswerResponse DeleteResponse(string responseId, int userId);

        Task<bool> SaveFormProperties(SurveyResponseBO request);

        SurveyAnswerResponse GetSurveyAnswerResponse(string responseId);

        SurveyAnswerResponse GetSurveyAnswerResponse(string responseId, int UserId);

        IEnumerable<SurveyResponse> GetAllResponsesContainingFields(IDictionary<int, FieldDigest> gridFields);

		FormResponseDetail GetHierarchialResponsesByResponseId(string responseId, bool includeDeletedRecords = false);

		void NotifyConsistencyService(string responseId, int responseStatus, RecordStatusChangeReason reasonForStatusChange);
	}
}
