using System.Collections.Generic;
using System.Threading.Tasks;
using Epi.DataPersistence.Constants;
using Epi.DataPersistence.DataStructures;
using Epi.FormMetadata.DataStructures;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.Message;

namespace Epi.DataPersistence.Common.Interfaces
{
    public interface ISurveyPersistenceFacade
    {
        bool DoesResponseExist(string childFormId, string parentResponseId);

        bool DoChildrenExistForResponseId(string parentResponseId);

        bool UpdateResponseStatus(string responseId, int recordStatus, RecordStatusChangeReason reasonForStatusChange);

		int GetFormResponseCount(string formId, bool includeDeletedRecords = false);

        FormResponseDetail GetFormResponseState(string responseId);

        FormResponseDetail GetFormResponseByResponseId(string responseId);

        bool SaveResponse(SurveyResponseBO surveyResponseBO);
		bool InsertResponse(SurveyResponseBO surveyResponseBO);

        PageResponseDetail ReadSurveyAnswerByResponseID(string surveyId, string responseId, int pageId);

        SurveyAnswerResponse DeleteResponse(string responseId, int userId);

        bool SaveFormProperties(SurveyResponseBO request);

        //SurveyAnswerResponse GetSurveyAnswerResponse(string responseId);

        //SurveyAnswerResponse GetSurveyAnswerResponse(string responseId, int UserId);

        IEnumerable<SurveyResponse> GetAllResponsesContainingFields(IDictionary<int, FieldDigest> gridFields, int pageSize = 0, int pageNumber = 0);

        FormResponseDetail GetHierarchialResponseIdsByResponseId(string responseId, bool includeDeletedRecords = false);

		FormResponseDetail GetHierarchialResponsesByResponseId(string responseId, bool includeDeletedRecords = false);

        void NotifyConsistencyService(string responseId, int responseStatus, RecordStatusChangeReason reasonForStatusChange);
	}
}
