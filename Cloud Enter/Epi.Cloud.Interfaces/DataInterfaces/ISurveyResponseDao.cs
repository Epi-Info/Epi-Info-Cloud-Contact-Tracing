using System;
using System.Collections.Generic;
using Epi.DataPersistence.Constants;
using Epi.Web.Enter.Common.BusinessObject;
using Epi.Web.Enter.Common.Criteria;

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
        List<SurveyResponseBO> GetSurveyResponseSize(List<string> surveyResponseIdList, Guid userPublishKey, int pageNumber = -1, int pageSize = -1, int responseMaxSize = -1);

        SurveyResponseBO GetSurveyResponseState(string responseId);

        /// <summary>
        /// Gets a specific SurveyResponse.
        /// </summary>
        /// <param name="surveyResponseId">Unique SurveyResponse identifier.</param>
        /// <returns>SurveyResponse.</returns>
        List<SurveyResponseBO> GetSurveyResponseBySurveyId(List<string> surveyIdList, Guid userPublishKey, int pageNumber = -1, int pageSize = -1);

        List<SurveyResponseBO> GetSurveyResponseBySurveyIdSize(List<string> surveyIdList, Guid userPublishKey, int pageNumber = -1, int pageSize = -1, int responseMaxSize = -1);

        /// <summary>
        /// Get SurveyResponses based on criteria.
        /// </summary>
        /// <param name="SurveyResponseId">Unique SurveyResponse identifier.</param>
        /// <returns>SurveyResponse.</returns>
        List<SurveyResponseBO> GetSurveyResponse(List<string> surveyAnswerIdList, string surveyId, DateTime dateCompleted, bool isDraftMode = false, int statusId = -1, int pageNumber = -1, int pageSize = -1);

        List<SurveyResponseBO> GetSurveyResponseSize(List<string> surveyAnswerIdList, string pSurveyId, DateTime dateCompleted, bool isDraftMode = false, int statusId = -1, int pageNumber = -1, int pageSize = -1, int responseMaxSize = -1);
        /// <summary>
        /// Gets a sorted list of all SurveyResponses.
        /// </summary>
        /// <param name="sortExpression">Sort order.</param>
        /// <returns>Sorted list of SurveyResponses.</returns>
        // List<SurveyResponseBO> GetSurveyResponses(string sortExpression = "SurveyResponseId ASC");

        /// <summary>
        /// Gets SurveyResponse given an order.
        /// </summary>
        /// <param name="orderId">Unique order identifier.</param>
        /// <returns>SurveyResponse.</returns>
        // SurveyResponseBO GetSurveyResponseByOrder(int orderId);

        /// <summary>
        /// Gets SurveyResponses with order statistics in given sort order.
        /// </summary>
        /// <param name="SurveyResponses">SurveyResponse list.</param>
        /// <param name="sortExpression">Sort order.</param>
        /// <returns>Sorted list of SurveyResponses with order statistics.</returns>
        //   List<SurveyResponseBO> GetSurveyResponsesWithOrderStatistics(string sortExpression);

        /// <summary>
        /// Inserts a new SurveyResponse. 
        /// </summary>
        /// <remarks>
        /// Following insert, SurveyResponse object will contain the new identifier.
        /// </remarks>
        /// <param name="surveyResponse">SurveyResponse.</param>
        void InsertSurveyResponse(SurveyResponseBO surveyResponse);
        /// <summary>
        /// Inserts a new SurveyResponse. 
        /// </summary>
        /// <remarks>
        /// Following insert, SurveyResponse object will contain the new identifier.
        /// </remarks>
        /// <param name="surveyResponse">SurveyResponse.</param>
        void InsertChildSurveyResponse(SurveyResponseBO surveyResponse);
        /// <summary>
        /// Updates a SurveyResponse.
        /// </summary>
        /// <param name="surveyResponse">SurveyResponse.</param>
        void UpdateSurveyResponse(SurveyResponseBO surveyResponse);

        /// <summary>
        /// Deletes a SurveyResponse
        /// </summary>
        /// <param name="surveyResponse">SurveyResponse.</param>
        void DeleteSurveyResponse(SurveyResponseBO surveyResponse);
        void UpdatePassCode(UserAuthenticationRequestBO passcodeBO);
        UserAuthenticationResponseBO GetAuthenticationResponse(UserAuthenticationRequestBO passcodeBO);
        List<SurveyResponseBO> GetFormResponseByFormId(string formId, int pageNumber, int pageSize);
        List<SurveyResponseBO> GetFormResponseByFormId(SurveyAnswerCriteria criteria);
        int GetFormResponseCount(string formId);
        int GetFormResponseCount(SurveyAnswerCriteria criteria);
        string GetResponseParentId(string responseId);
        SurveyResponseBO GetSingleResponse(string responseId);
        List<SurveyResponseBO> GetResponsesHierarchyIdsByRootId(string rootId);
        void DeleteSingleSurveyResponse(SurveyResponseBO surveyResponse);
        SurveyResponseBO GetFormResponseByParentRecordId(string responseId);
        List<SurveyResponseBO> GetAncestorResponseIdsByChildId(string childId);
        List<SurveyResponseBO> GetResponsesByRelatedFormId(string responseId, string SurveyId);
        List<SurveyResponseBO> GetResponsesByRelatedFormId(string responseId, SurveyAnswerCriteria Criteria);
        void DeleteSurveyResponseInEditMode(SurveyResponseBO surveyResponse);
        SurveyResponseBO GetResponse(string responseId);
        void DeleteResponse(ResponseBO responseBO);
        void InsertResponse(ResponseBO item);
        bool DoChildrenExistForResponseId(Guid responseId);
        //bool DoesResponseExist(SurveyAnswerCriteria Criteria);

        //bool DoesResponseExist(Guid responseId);
        bool HasResponse(SurveyAnswerCriteria criteria);
        void UpdateRecordStatus(SurveyResponseBO surveyResponseBO);
        void UpdateRecordStatus(string responseId, int status, RecordStatusChangeReason reasonForStatusChange);
        int GetDataAccessRule(string formId, int userId);
    }
}
