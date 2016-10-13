using System.Collections.Generic;
using System.Threading.Tasks;
using Epi.Cloud.Common.Constants;
using Epi.Cloud.DataEntryServices.Model;
using Epi.DataPersistence.DataStructures;
using Epi.FormMetadata.DataStructures;
using Epi.Web.Enter.Common.BusinessObject;
using Epi.Web.Enter.Common.DTO;
using Epi.Web.Enter.Common.Message;

namespace Epi.Cloud.DataEntryServices.Facade
{
	public interface ISurveyPersistenceFacade
    {
        //Insert new record  survey response data in to table storage.

        bool DoChildrenExistForResponseId(string responseId);

        bool UpdateResponseStatus(string responseId, int recordStatus, RecordStatusChangeReason reasonForStatusChange);

		int GetFormResponseCount(string formId, bool includeDeletedRecords = false);

        FormResponseDetail GetFormResponseState(string responseId);

        FormResponseDetail GetFormResponseByResponseId(string responseId);

        Task<bool> InsertResponseAsync(SurveyInfoModel surveyInfoModel, string responseId, MvcDynamicForms.Form form, Epi.Web.Enter.Common.DTO.SurveyAnswerDTO surveyAnswerDTO, bool IsSubmited, bool IsSaved, int PageNumber, int UserId);

        Task<bool> InsertChildResponseAsync(SurveyResponseBO surveyResponseBO);
        PageResponseDetail ReadSurveyAnswerByResponseID(string surveyId, string responseId, int pageId);

        SurveyAnswerResponse DeleteResponse(string responseId, int userId);

        bool SaveFormProperties(SurveyResponseBO request);
        SurveyAnswerResponse GetSurveyAnswerResponse(string responseId);
        SurveyAnswerResponse GetSurveyAnswerResponse(string responseId, int UserId);
        IEnumerable<SurveyResponse> GetAllResponsesContainingFields(IDictionary<int, FieldDigest> gridFields);
		//FormsHierarchyDTO GetChildRecordByChildFormId(string childFormId, string relateParentId, IDictionary<int, FieldDigest> gridFields);

		FormResponseDetail GetHierarchialResponsesByResponseId(string responseId, bool includeDeletedRecords = false);

		void NotifyConsistencyService(string responseId, int responseStatus, RecordStatusChangeReason reasonForStatusChange);
	}
}
