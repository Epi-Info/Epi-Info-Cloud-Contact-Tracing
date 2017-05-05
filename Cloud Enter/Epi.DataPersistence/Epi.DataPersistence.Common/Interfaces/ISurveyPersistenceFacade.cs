using System.Collections.Generic;
using Epi.DataPersistence.Constants;
using Epi.DataPersistence.DataStructures;
using Epi.FormMetadata.DataStructures;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.Message;
using Epi.Common.Core.Interfaces;

namespace Epi.DataPersistence.Common.Interfaces
{
    public interface ISurveyPersistenceFacade
    {
        FormResponseDetail GetFormResponseState(IResponseContext responseContext);

        bool DoChildResponsesExist(IResponseContext responseContext, bool includeDeletedRecords = false);

        bool UpdateResponseStatus(IResponseContext responseContext, int recordStatus, RecordStatusChangeReason reasonForStatusChange);

        int GetFormResponseCount(string formId, bool includeDeletedRecords = false);
        FormResponseDetail GetFormResponseByResponseId(IResponseContext responseContext);

        bool SaveResponse(SurveyResponseBO surveyResponseBO);


        SurveyAnswerResponse DeleteResponse(IResponseContext responseContext);

        bool SaveFormResponseProperties(SurveyResponseBO request);

        //IEnumerable<SurveyResponse> GetAllResponsesWithCriteria(IResponseContext responceContext, IDictionary<int, FieldDigest> gridFields, IDictionary<int, KeyValuePair<FieldDigest, string>> searchFields, int pageSize = 0, int pageNumber = 0);
        IEnumerable<FormResponseDetail> GetAllResponsesWithCriteria(IResponseContext responceContext, IDictionary<int, FieldDigest> gridFields, IDictionary<int, KeyValuePair<FieldDigest, string>> searchFields, int pageSize = 0, int pageNumber = 0);

        FormResponseDetail GetHierarchialResponsesByResponseId(IResponseContext responceContext, bool includeDeletedRecords = false);

        void NotifyConsistencyService(IResponseContext responceContext, int responseStatus, RecordStatusChangeReason reasonForStatusChange);
    }
}
