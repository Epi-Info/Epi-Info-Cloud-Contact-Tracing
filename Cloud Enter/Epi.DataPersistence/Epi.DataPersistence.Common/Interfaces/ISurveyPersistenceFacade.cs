using System.Collections.Generic;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.Message;
using Epi.Common.Core.DataStructures;
using Epi.Common.Core.Interfaces;
using Epi.DataPersistence.Constants;
using Epi.DataPersistence.DataStructures;

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

        //bool SaveFormResponseProperties(SurveyResponseBO request);

        IEnumerable<FormResponseDetail> GetAllResponsesWithCriteria(ResponseGridQueryCriteria responseGridQueryCriteria);

        FormResponseDetail GetHierarchicalResponsesByResponseId(IResponseContext responceContext, bool includeDeletedRecords = false);

        void NotifyConsistencyService(IResponseContext responceContext, int responseStatus, RecordStatusChangeReason reasonForStatusChange);

        void NotifyConsistencyService(FormResponseDetail hierarchicalFormResponseDetail);
    }
}
