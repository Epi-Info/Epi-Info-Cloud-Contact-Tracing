using System;
using System.Collections.Generic;
using Epi.Cloud.Common.BusinessObjects;
using Epi.Cloud.Common.Criteria;
using Epi.DataPersistence.Constants;

namespace Epi.Cloud.Interfaces.DataInterfaces
{
    /// <summary>
    /// Defines methods to access SurveyResponses.
    /// </summary>
    /// <remarks>
    /// This is a database-independent interface. Implementations are database specific.
    /// </remarks>
    public interface ISurveyResponseDao
    {
        /// <summary>
        /// Gets a specific SurveyResponse.
        /// </summary>
        /// <param name="surveyResponseId">Unique SurveyResponse identifier.</param>
        /// <returns>SurveyResponse.</returns>
        List<SurveyResponseBO> GetSurveyResponse(List<string> surveyResponseIdList, Guid userPublishKey, int pageNumber = -1, int pageSize = -1);
       
        /// <summary>
        /// GetSurveyResponseState
        /// </summary>
        /// <param name="responseId"></param>
        /// <returns></returns>
        SurveyResponseBO GetSurveyResponseState(string responseId);

        /// <summary>
        /// Inserts a new SurveyResponse. 
        /// </summary>
        /// <remarks>
        /// Following insert, SurveyResponse object will contain the new identifier.
        /// </remarks>
        /// <param name="surveyResponse">SurveyResponse.</param>
        void InsertSurveyResponse(SurveyResponseBO surveyResponse);

        /// <summary>
        /// Updates a SurveyResponse.
        /// </summary>
        /// <param name="surveyResponse">SurveyResponse.</param>
        void UpdateSurveyResponse(SurveyResponseBO surveyResponse);

        /// <summary>
        /// UpdatePassCode
        /// </summary>
        /// <param name="passcodeBO"></param>
        void UpdatePassCode(UserAuthenticationRequestBO passcodeBO);

        /// <summary>
        /// GetAuthenticationResponse
        /// </summary>
        /// <param name="passcodeBO"></param>
        /// <returns></returns>
        UserAuthenticationResponseBO GetAuthenticationResponse(UserAuthenticationRequestBO passcodeBO);

        /// <summary>
        /// GetFormResponseByFormId
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        List<SurveyResponseBO> GetFormResponseByFormId(SurveyAnswerCriteria criteria);

        /// <summary>
        /// GetFormResponseCount
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        int GetFormResponseCount(string formId);

        /// <summary>
        /// GetFormResponseCount
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        int GetFormResponseCount(SurveyAnswerCriteria criteria);

        /// <summary>
        /// GetResponsesHierarchyIdsByRootId
        /// </summary>
        /// <param name="rootId"></param>
        /// <returns></returns>
        List<SurveyResponseBO> GetResponsesHierarchyIdsByRootId(string rootId);

        /// <summary>
        /// GetResponse
        /// </summary>
        /// <param name="responseId"></param>
        /// <returns></returns>
        SurveyResponseBO GetResponse(string responseId);

        /// <summary>
        /// HasResponse
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        bool HasResponse(SurveyAnswerCriteria criteria);

        /// <summary>
        /// UpdateRecordStatus
        /// </summary>
        /// <param name="responseId"></param>
        /// <param name="status"></param>
        /// <param name="reasonForStatusChange"></param>
        void UpdateRecordStatus(string responseId, int status, RecordStatusChangeReason reasonForStatusChange);

        /// <summary>
        /// GetDataAccessRule
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        int GetDataAccessRule(string formId, int userId);
    }
}
