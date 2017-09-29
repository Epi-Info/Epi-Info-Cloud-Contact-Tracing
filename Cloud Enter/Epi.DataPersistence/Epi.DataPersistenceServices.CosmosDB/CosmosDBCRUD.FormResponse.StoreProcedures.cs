using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Epi.Cloud.Resources;
using Epi.Cloud.Resources.Constants;
using Epi.Common.Core.DataStructures;
using Epi.PersistenceServices.CosmosDB;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

namespace Epi.DataPersistenceServices.CosmosDB
{
    public partial class CosmosDBCRUD
    {
        public int? continuationToken = null;

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
        private async Task<List<FormResponseProperties>> GetAllRootResponses(ResponseGridQueryCriteria responseGridQueryCriteria, string query,
            string continuationToken = null, int skip = 0)
        {
            string collectionId = responseGridQueryCriteria.ResponseContext.RootFormName;
            string sortKey = responseGridQueryCriteria.SortByField;
            bool isSortAscending = responseGridQueryCriteria.IsSortedAscending;
            var queryResult = await ExecuteSPAsync(collectionId, spGetGridContent, udfSharingRules, udfWildCardCompare,
                                                   query, sortKey, isSortAscending, continuationToken, skip);
            return queryResult;
        }

        internal class QueryResult
        {
            public Document[] Result { get; set; }
            public string ContinuationToken { get; set; }
            public int Skip { get; set; }
            public string Message { get; set; }
            public string Trace { get; set; }
        }

        internal class OrderByResult
        {
            public Document[] Result { get; set; }
            public int? Continuation { get; set; }
        }

        /// <summary>
        /// ExecuteSPAsync
        /// </summary>
        /// <param name="collectionId"></param>
        /// <param name="spId"></param>
        /// <param name="surveyId"></param>
        /// <returns></returns>
        private async Task<List<FormResponseProperties>> ExecuteSPAsync(string collectionId, string spId, string udfSharingRulesId, string udfWildCardCompareId, string query,
                                                                        string sortKey, bool isSortAscending, string continuationToken, int skip)
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
                return ExecuteQuery(spId, spUri, query, sortKey, isSortAscending, continuationToken, skip);
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
                        return ExecuteQuery(spId, spUri, query, sortKey, isSortAscending, continuationToken, skip);
                    }
                    catch (Exception ex2)
                    {
                    }
                }
            }
            return null;
        }

        class SpParams
        {
            public string query;
            public string sortKey;
            public bool isSortAscending;
            public string continuationToken;
            public int skip;
        }

        private List<FormResponseProperties> ExecuteQuery(string spId, Uri spUri, string query, 
                                                          string sortKey, bool isSortAscending, string continuationToken, int skip)
        {
            var spParams = new SpParams { query = query,
                sortKey = sortKey, isSortAscending = isSortAscending,
                continuationToken = continuationToken, skip = skip };
            var json = JsonConvert.SerializeObject(spParams);

            var formResponseList = new List<FormResponseProperties>();
            if (spId == spGetGridContent)
            {
                QueryResult queryResult = Client.ExecuteStoredProcedureAsync<QueryResult>(spUri, query, sortKey, isSortAscending, continuationToken, skip).Result;

                foreach (var doc in queryResult.Result)
                {
                    FormResponseProperties formResponse = (dynamic)doc;
                    formResponseList.Add(formResponse);
                }
            }
            else
            {
                do
                {
                    var spResponse = Client.ExecuteStoredProcedureAsync<OrderByResult>(spUri, query).Result;
                    foreach (var doc in spResponse.Response.Result)
                    {
                        FormResponseProperties formResponse = (dynamic)doc;
                        formResponseList.Add(formResponse);
                    }
                } while (continuationToken != null);
            }

            return formResponseList;
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
