using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epi.Cloud.Resources;
using Epi.Cloud.Resources.Constants;
using Epi.Common.Core.DataStructures;
using Epi.PersistenceServices.CosmosDB;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using static Epi.Cloud.Common.Constants.Constant;

namespace Epi.DataPersistenceServices.CosmosDB
{
    public partial class CosmosDBCRUD
    {
        private const string spGetRecordsBySurveyId = "GetRecordsBySurveyId";
        private const string spGetGridContent = "getGridContent";
        private const string udfWildCardCompare = "WildCardCompare";
        private const string udfSharingRules = "SharingRules";

        /// <summary>
        /// Execute DB SP-Get all records by FormName (aka: collectionId) 
        /// </summary>
        /// <param name="collectionId"></param>
        /// <param name="spId"></param>
        /// <param name="udfWildCardCompareId"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        private async Task<ResponseGridQueryPropertiesResult> GetAllRootResponses(ResponseGridQueryCriteria responseGridQueryCriteria, string query,
            int pageNumber, int responsesPerPage,
            string querySetToken)
        {
            string collectionId = responseGridQueryCriteria.ResponseContext.RootFormName;
            string sortKey = responseGridQueryCriteria.SortByField;
            bool isSortAscending = responseGridQueryCriteria.IsSortedAscending;

            var queryResult = await ExecuteSPAsync(collectionId, spGetGridContent, udfSharingRules, udfWildCardCompare,
                                                   query, sortKey, isSortAscending, pageNumber, responsesPerPage,
                                                   querySetToken);
            return queryResult;
        }

        internal class QueryResult
        {
            public Document[] Result { get; set; }
            public string QuerySetToken { get; set; }
            public int NumberOfResponsesReturnedByQuery { get; set; }
            public int TotalSizeOfResponsesReturnedByQuery { get; set; }

            public int NumberOfResponsesPerPage { get; set; }
            public int NumberOfResponsesOnSelectedPage { get; set; }
            public int TotalSizeOfResponsesOnSelectedPage { get; set; }

            public int PageNumber { get; set; }
            public int NumberOfPages { get; set; }
            public string ContinuationToken { get; set; }
            public int Skip { get; set; }
            public string Message { get; set; }
            public string Trace { get; set; }
        }

        /// <summary>
        /// ExecuteSPAsync
        /// </summary>
        /// <param name="collectionId"></param>
        /// <param name="spId"></param>
        /// <param name="surveyId"></param>
        /// <returns></returns>
        private async Task<ResponseGridQueryPropertiesResult> ExecuteSPAsync(string collectionId, string spId, string udfSharingRulesId, string udfWildCardCompareId, string query,
                                                                        string sortKey, bool isSortAscending, int pageNumber, int responsesPerPage,
                                                                        string querySetToken)
        {
            RequestOptions option = new RequestOptions();
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, collectionId);
            var formResponse = new FormResponseProperties();
            // Create Stored Procedure Uri
            Uri spUri = UriFactory.CreateStoredProcedureUri(DatabaseName, collectionId, spId);

            // Create the User Defined Function Uris
            Uri udfSharingRulesUri = UriFactory.CreateUserDefinedFunctionUri(DatabaseName, collectionId, udfSharingRulesId);
            Uri udfWildCardUri = UriFactory.CreateUserDefinedFunctionUri(DatabaseName, collectionId, udfWildCardCompareId);

            try
            {
                return ExecuteQuery(spId, spUri, query, sortKey, isSortAscending, pageNumber, responsesPerPage,
                                    querySetToken);
            }
            catch (Exception ex)
            {
                var errorCode = ((DocumentClientException)ex.InnerException).Error.Code;
                if (errorCode == "NotFound" || errorCode == "BadRequest")
                {
                    if (await DoesStoredProcedureExist(spUri) == false)
                    {
                        var createSPResponse = await CreateSPAsync(collectionUri, spId);
                    }
                    if (await DoesUserDefinedFunctionExist(udfSharingRulesUri) == false)
                    {
                        var createUDFResponse = await CreateUDFAsync(collectionUri, udfSharingRulesId, CosmosDBUDFKeys.udfSharingRules);
                    }
                    if (await DoesUserDefinedFunctionExist(udfWildCardUri) == false)
                    {
                        var createUDFResponse = await CreateUDFAsync(collectionUri, udfWildCardCompareId, CosmosDBUDFKeys.udfWildCardCompare);
                    }

                    try
                    {
                        return ExecuteQuery(spId, spUri, query, sortKey, isSortAscending, pageNumber, responsesPerPage,
                                            querySetToken);
                    }
                    catch (Exception ex2)
                    {
                        throw;
                    }
                }
            }
            return null;
        }

        private ResponseGridQueryPropertiesResult ExecuteQuery(string spId, Uri spUri, string query,
                                                          string sortKey, bool isSortAscending,
                                                          int pageNumber, int responsesPerPage,
                                                          string querySetToken)
        {
            var responsePropertiesList = new List<FormResponseProperties>();
            QueryResult queryResult = Client.ExecuteStoredProcedureAsync<QueryResult>(spUri, query, sortKey, isSortAscending,
                         pageNumber, responsesPerPage, querySetToken).Result;
            foreach (var doc in queryResult.Result)
            {
                FormResponseProperties formResponse = (dynamic)doc;
                responsePropertiesList.Add(formResponse);
            }
            var responseGridQueryPropertiesResult = new ResponseGridQueryPropertiesResult
            {
                ResponsePropertiesList = responsePropertiesList,
                QuerySetToken = queryResult.QuerySetToken,
                NumberOfResponsesReturnedByQuery = queryResult.NumberOfResponsesReturnedByQuery,
                NumberOfResponsesPerPage = queryResult.NumberOfResponsesPerPage,
                NumberOfResponsesOnSelectedPage = queryResult.NumberOfResponsesOnSelectedPage,
                PageNumber = pageNumber,
                NumberOfPages = queryResult.NumberOfPages
            };
            if (queryResult.Message == "Completed")
            {
                return responseGridQueryPropertiesResult;
            }
            else
            {
                throw new Exception(queryResult.Message);
                // ----------------------------------Error Handling ----------------------------------

                //string continuationToken = string.Empty;
                //int skip = 0;
                //bool isContinuationRequired = false;
                //bool isPostProcessingRequired = false;

                //do
                //{
                //queryResult = Client.ExecuteStoredProcedureAsync<QueryResult>(spUri, query, sortKey, isSortAscending,
                //                         pageNumber, responsesPerPage, querySetToken/*, continuationToken, skip,
                //                         isPostProcessingRequired).Result;

                //foreach (var doc in queryResult.Result)
                //{
                //    FormResponseProperties formResponse = (dynamic)doc;
                //    responsePropertiesList.Add(formResponse);
                //}
                //continuationToken = queryResult.ContinuationToken;
                //skip = queryResult.Skip;
                //    isContinuationRequired = !string.IsNullOrEmpty(continuationToken);
                //    isPostProcessingRequired |= isContinuationRequired;
                //} while (isContinuationRequired);

                //var responseGridQueryPropertiesResult = new ResponseGridQueryPropertiesResult
                //{
                //    ResponsePropertiesList = responsePropertiesList,
                //    QuerySetToken = queryResult.QuerySetToken,
                //    NumberOfResponsesReturnedByQuery = queryResult.NumberOfResponsesReturnedByQuery,
                //    NumberOfResponsesPerPage = queryResult.NumberOfResponsesPerPage,
                //    NumberOfResponsesOnSelectedPage = queryResult.NumberOfResponsesOnSelectedPage,
                //    PageNumber = pageNumber,
                //    NumberOfPages = queryResult.NumberOfPages
                //    //IsPostProcessingRequired = isPostProcessingRequired
                //};

                //if (isPostProcessingRequired)
                //{
                //    List<FormResponseProperties> pageResult = null;

                //    skip = pageNumber * responsesPerPage - responsesPerPage;
                //    sortKey = string.IsNullOrWhiteSpace(sortKey) ? MetaColumn.DateUpdated : sortKey;
                //    switch (sortKey)
                //    {
                //        case MetaColumn.UserEmail:
                //            pageResult = (isSortAscending
                //                         ? responsePropertiesList.OrderBy(r => r.UserName.ToUpperInvariant())
                //                         : responsePropertiesList.OrderByDescending(r => r.UserName.ToUpperInvariant()))
                //                         .Skip(skip).Take(responsesPerPage).ToList();

                //            break;
                //        case MetaColumn.IsDraftMode:
                //        case MetaColumn.Mode:
                //            pageResult = (isSortAscending
                //                         ? responsePropertiesList.OrderBy(r => r.IsDraftMode)
                //                         : responsePropertiesList.OrderByDescending(r => r.IsDraftMode))
                //                         .Skip(skip).Take(responsesPerPage).ToList();

                //            break;
                //        case MetaColumn.DateCreated:
                //            pageResult = (isSortAscending
                //                         ? responsePropertiesList.OrderBy(r => r.FirstSaveTime)
                //                         : responsePropertiesList.OrderByDescending(r => r.FirstSaveTime))
                //                         .Skip(skip).Take(responsesPerPage).ToList();

                //            break;
                //        case MetaColumn.DateUpdated:
                //            pageResult = (isSortAscending
                //                         ? responsePropertiesList.OrderBy(r => r.LastSaveTime)
                //                         : responsePropertiesList.OrderByDescending(r => r.LastSaveTime))
                //                         .Skip(skip).Take(responsesPerPage).ToList();

                //            break;
                //        default:
                //            pageResult = (isSortAscending
                //                         ? responsePropertiesList.OrderBy(r => r.ResponseQA.ContainsKey(sortKey) && r.ResponseQA[sortKey] != null ? r.ResponseQA[sortKey].ToUpperInvariant() : string.Empty)
                //                         : responsePropertiesList.OrderByDescending(r => r.ResponseQA.ContainsKey(sortKey) && r.ResponseQA[sortKey] != null ? r.ResponseQA[sortKey].ToUpperInvariant() : string.Empty))
                //                         .Skip(skip).Take(responsesPerPage).ToList();
                //            break;
                //    }
                //    var totalResponseCount = responsePropertiesList.Count();
                //    responseGridQueryPropertiesResult.NumberOfResponsesReturnedByQuery = totalResponseCount;
                //    responseGridQueryPropertiesResult.NumberOfResponsesOnSelectedPage = pageResult.Count();
                //    responseGridQueryPropertiesResult.ResponsePropertiesList = pageResult;
                //    responseGridQueryPropertiesResult.NumberOfPages = totalResponseCount / responsesPerPage + 1;
                //}
                return responseGridQueryPropertiesResult;
            }
        }

        public async Task<bool> DoesStoredProcedureExist(Uri spUri)
        {
            bool exists = false;
            try
            {
                var task = Client.ReadStoredProcedureAsync(spUri);
                while (!task.IsCompleted && !task.IsFaulted) Thread.Sleep(5);
                var spResult = await task;
                exists = spResult != null && !task.IsFaulted;
            }
            catch (Exception ex)
            {
                exists = false;
            }
            return exists;
        }


        public async Task<bool> DoesUserDefinedFunctionExist(Uri udfUri)
        {
            bool exists = false;
            try
            {
                var task = Client.ReadUserDefinedFunctionAsync(udfUri);
                while (!task.IsCompleted && !task.IsFaulted) Thread.Sleep(5);
                var udfResult = await task;
                exists = udfResult != null && !task.IsFaulted;
            }
            catch (Exception ex)
            {
                exists = false;
            }
            return exists;
        }

        /// <summary>
        /// Create Cosmos DB Stored Procedure
        /// </summary>
        /// <param name="collectionUri"></param>
        /// <param name="spId"></param>
        /// <returns></returns>
        private async Task<StoredProcedure> CreateSPAsync(Uri collectionUri, string spId)
        {

            try
            {
                var sprocBody = ResourceProvider.GetResourceString(ResourceNamespaces.CosmosDBSp, CosmosDBSPKeys.GetAllRecordsBySurveyID);
                var sprocDefinition = new StoredProcedure
                {
                    Id = spId,
                    Body = sprocBody
                };
                var result = ExecuteWithFollowOnAction(() => Client.CreateStoredProcedureAsync(collectionUri, sprocDefinition));
                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        /// <summary>
        /// Create Cosmos DB Stored Procedure
        /// </summary>
        /// <param name="collectionUri"></param>
        /// <param name="udfId"></param>
        /// <returns></returns>
        private async Task<UserDefinedFunction> CreateUDFAsync(Uri collectionUri, string udfId, string resourceName)
        {
            try
            {
                var udfBody = ResourceProvider.GetResourceString(ResourceNamespaces.CosmosDBSp, resourceName);
                var udfDefinition = new UserDefinedFunction
                {
                    Id = udfId,
                    Body = udfBody
                };
                var udfTask = Client.CreateUserDefinedFunctionAsync(collectionUri, udfDefinition);
                while (!udfTask.IsCompleted && !udfTask.IsFaulted) Thread.Sleep(5);
                if (udfTask.IsFaulted) throw new Exception("CreateUserDefinedFunction faulted");
                var response = await udfTask;
                return response.Resource;
            }
            catch (Exception ex)
            {

            }
            return null;
        }
    }
}
