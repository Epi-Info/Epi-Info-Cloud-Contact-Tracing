using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Epi.Cloud.Resources;
using Epi.Cloud.Resources.Constants;
using Epi.PersistenceServices.DocumentDB;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Epi.DataPersistenceServices.DocumentDB
{
    public partial class DocumentDbCRUD
    {
        public int? continuationToken = null;

        private const string spGetRecordsBySurveyId = "GetRecordsBySurveyId";
        private const string udfWildCardCompare = "WildCardCompare";

        /// <summary>
        /// Execute DB SP-Get all records by surveyID 
        /// </summary>
        /// <param name="collectionId"></param>
        /// <param name="spId"></param>
        /// <param name="surveyID"></param>
        /// <returns></returns>
        private async Task<List<FormResponseProperties>> GetAllRecordsBySurveyId(string collectionId, string spId, string udfId, string query)
        {
            return await ExecuteSPAsync(collectionId, spId, udfId, query);
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
        private async Task<List<FormResponseProperties>> ExecuteSPAsync(string collectionId, string spId, string udfId, string query)
        {
            RequestOptions option = new RequestOptions();
            var formResponseList = new List<FormResponseProperties>();
            var formResponse = new FormResponseProperties();
            // Create SP Uri
            string spUri = UriFactory.CreateStoredProcedureUri(DatabaseName, collectionId, spId).ToString();
            string udfUri = UriFactory.CreateUserDefinedFunctionUri(DatabaseName, collectionId, udfId).ToString();
            try
            {
                do
                {
                    var spResponse = Client.ExecuteStoredProcedureAsync<OrderByResult>(spUri, query).Result;
                    foreach (var doc in spResponse.Response.Result)
                    {
                        formResponse = (dynamic)doc;
                        formResponseList.Add(formResponse);
                    }
                } while (continuationToken != null);

                return formResponseList;
            }
            catch (Exception ex)
            {
                var errorCode = ((DocumentClientException)ex.InnerException).Error.Code;
                if (errorCode == "NotFound" /* || errorCode == "BadRequest" */ )
                {
                    var createSPResponse = await CreateSPAsync(spUri, spId);
                    //var createUDFResponse = await CreateUDFAsync(udfUri, udfId);

                    //Execute SP 
                    await ExecuteSPAsync(collectionId, spId, udfId, query);
                }
            }
            return null;
        }

        /// <summary>
        /// Create Document Db Stored Procedure
        /// </summary>
        /// <param name="spSelfLink"></param>
        /// <param name="spId"></param>
        /// <returns></returns>
        private async Task<StoredProcedure> CreateSPAsync(string spSelfLink, string spId)
        {

            try
            {
                var sprocBody = ResourceProvider.GetResourceString(ResourceNamespaces.DocumentDBSp, DocumentDBSPKeys.GetAllRecordsBySurveyID);
                var sprocDefinition = new StoredProcedure
                {
                    Id = spId,
                    Body = sprocBody
                };
                var result = ExecuteWithFollowOnAction(() => Client.CreateStoredProcedureAsync(spSelfLink, sprocDefinition));
                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        /// <summary>
        /// Create Document Db Stored Procedure
        /// </summary>
        /// <param name="udfSelfLink"></param>
        /// <param name="udfId"></param>
        /// <returns></returns>
        private async Task<UserDefinedFunction> CreateUDFAsync(string udfSelfLink, string udfId)
        {
            try
            {
                var udfBody = ResourceProvider.GetResourceString(ResourceNamespaces.DocumentDBSp, DocumentDBUDFKeys.udfWildCardCompare);
                var udfDefinition = new UserDefinedFunction
                {
                    Id = udfId,
                    Body = udfBody
                };
                var udfTask = Client.CreateUserDefinedFunctionAsync(udfSelfLink, udfDefinition);
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
