using System;
using System.Collections.Generic;
using Epi.Cloud.Common.BusinessObjects;

namespace Epi.Cloud.Interfaces.DataInterfaces
{
	/// <summary>
	/// Defines methods to access SurveyInfos.
	/// </summary>
	/// <remarks>
	/// This is a database-independent interface. Implementations are database specific.
	/// </remarks>
	public interface ISurveyInfoDao
	{
		/// <summary>
		/// Gets SurveyInfo
		/// </summary>
		/// <param name="surveyId"></param>
		/// <returns></returns>
		SurveyInfoBO GetSurveyInfo(string surveyId);

		/// <summary>
		/// Gets SurveyInfo based on a list of ids
		/// </summary>
		/// <param name="SurveyInfoId">Unique SurveyInfo identifier.</param>
		/// <returns>SurveyInfo.</returns>
		List<SurveyInfoBO> GetSurveyInfo(List<string> surveyInfoIdList, int gridPageNumber = -1, int gridPageSize = -1);

		List<SurveyInfoBO> GetSurveyInfoByOrgKeyAndPublishKey(string SurveyId, string pOrganizationKey, Guid publishKey);

		List<SurveyInfoBO> GetSurveyInfoByOrgKey(string SurveyId, string pOrganizationKey);

		/// <summary>
		/// Gets SurveyInfo Size data based on a list of ids
		/// </summary>
		/// <param name="surveyInfoIdList">List of unique SurveyInfo identifiers.</param>
		/// <returns>PageInfoBO.</returns>
		List<SurveyInfoBO> GetSurveySizeInfo(List<string> surveyInfoIdList, int gridPageNumber = -1, int gridPageSize = -1, int ResponsesTotalsize = -1);


		/// <summary>
		/// Gets SurveyInfo based on criteria
		/// </summary>
		/// <param name="surveyInfoIdList">List of unique SurveyInfo identifiers.</param>
		/// <returns>SurveyInfo.</returns>
		List<SurveyInfoBO> GetSurveyInfo(List<string> surveyInfoIdList, DateTime pClosingDate, string Okey, int pSurveyType = -1, int gridPageNumber = -1, int gridPageSize = -1);

		/// <summary>
		/// Gets SurveyInfo Size data based on a list of ids
		/// </summary>
		/// <param name="surveyInfoIdList">List of unique SurveyInfo identifiers.</param>
		/// <returns>PageInfoBO.</returns>
		List<SurveyInfoBO> GetSurveySizeInfo(List<string> surveyInfoIdList, DateTime pClosingDate, string Okey, int pSurveyType = -1, int gridPageNumber = -1, int gridPageSize = -1, int ResponsesTotalsize = -1);

		/// <summary>
		/// Gets a sorted list of all SurveyInfos.
		/// </summary>
		/// <param name="sortExpression">Sort order.</param>
		/// <returns>Sorted list of SurveyInfos.</returns>
		// List<SurveyInfoBO> GetSurveyInfos(string sortExpression = "SurveyInfoId ASC");

		/// <summary>
		/// Gets SurveyInfo given an order.
		/// </summary>
		/// <param name="orderId">Unique order identifier.</param>
		/// <returns>SurveyInfo.</returns>
		// SurveyInfoBO GetSurveyInfoByOrder(int orderId);

		/// <summary>
		/// Gets SurveyInfos with order statistics in given sort order.
		/// </summary>
		/// <param name="SurveyInfos">SurveyInfo list.</param>
		/// <param name="sortExpression">Sort order.</param>
		/// <returns>Sorted list of SurveyInfos with order statistics.</returns>
		//   List<SurveyInfoBO> GetSurveyInfosWithOrderStatistics(string sortExpression);

		/// <summary>
		/// Inserts a new SurveyInfo. 
		/// </summary>
		/// <remarks>
		/// Following insert, SurveyInfo object will contain the new identifier.
		/// </remarks>
		/// <param name="SurveyInfo">SurveyInfo.</param>
		void InsertSurveyInfo(SurveyInfoBO SurveyInfo);

		/// <summary>
		/// Updates a SurveyInfo.
		/// </summary>
		/// <param name="SurveyInfo">SurveyInfo.</param>
		void UpdateSurveyInfo(SurveyInfoBO SurveyInfo);

		/// <summary>
		/// Deletes a SurveyInfo
		/// </summary>
		/// <param name="SurveyInfo">SurveyInfo.</param>
		void DeleteSurveyInfo(SurveyInfoBO SurveyInfo);

		/// <summary>
		/// Deletes a SurveyInfo
		/// </summary>
		/// <param name="SurveyInfo">SurveyInfo.</param>
		List<SurveyInfoBO> GetChildInfoByParentId(string ParentFormId, int ViewId);
		List<SurveyInfoBO> GetFormsHierarchyIdsByRootId(string RootId);
		void InsertFormDefaultSettings(string FormId, bool IsSqlProject, List<string> ControlsNameList);
		void UpdateParentId(string SurveyId, int ViewId, string ParentFormId);
		void InsertConnectionString(DbConnectionStringBO ConnectionString);
		void UpdateConnectionString(DbConnectionStringBO ConnectionString);
	}
}
