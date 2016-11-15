using System;
using System.Collections.Generic;
using System.Linq;
using Epi.DataPersistence.Constants;
using Microsoft.Azure.Documents.Client;
using static Epi.PersistenceServices.DocumentDB.DataStructures;

namespace Epi.DataPersistenceServices.DocumentDB
{
    public partial class SurveyResponseCRUD
    {
        #region Get hierarchial New Inprocess responses by ResponseId
        /// <summary>
        /// GetHierarchialResponsesByResponseId
        /// </summary>
        /// <returns></returns>
        public HierarchicalDocumentResponseProperties CascadeAction(string responseId, ActionContext actionContext, Action<FormResponseProperties, ActionContext> action, Action<ActionContext> endAction = null)
        {
            HierarchicalDocumentResponseProperties hierarchicalDocumentResponseProperties = new HierarchicalDocumentResponseProperties();
            Uri formInfoCollectionUri = GetCollectionUri(FormInfoCollectionName);
            var documentResponseProperties = ReadAllNewInProcessResponsesByExpression(Expression("GlobalRecordID", EQ, responseId), formInfoCollectionUri).SingleOrDefault();
            if (documentResponseProperties != null)
            {
                hierarchicalDocumentResponseProperties.FormResponseProperties = documentResponseProperties.FormResponseProperties;
                hierarchicalDocumentResponseProperties.PageResponsePropertiesList = documentResponseProperties.PageResponsePropertiesList;
                hierarchicalDocumentResponseProperties.ChildResponseList = GetChildNewInProcessResponses(responseId, formInfoCollectionUri);
            }
            return hierarchicalDocumentResponseProperties;
        }

        private List<HierarchicalDocumentResponseProperties> GetChildNewInProcessResponses(string parentResponseId, Uri formInfoCollectionUri)
        {
            var childResponseList = new List<HierarchicalDocumentResponseProperties>();
            var documentResponsePropertiesList = ReadAllNewInProcessResponsesByExpression(Expression("RelateParentId", EQ, parentResponseId), formInfoCollectionUri);
            foreach (var documentResponseProperties in documentResponsePropertiesList)
            {
                var formResponseProperties = documentResponseProperties.FormResponseProperties;
                var childResponse = new HierarchicalDocumentResponseProperties();
                childResponse.FormResponseProperties = documentResponseProperties.FormResponseProperties;
                childResponse.PageResponsePropertiesList = documentResponseProperties.PageResponsePropertiesList;
                childResponseList.Add(childResponse);

                childResponse.ChildResponseList = GetChildNewInProcessResponses(documentResponseProperties.FormResponseProperties.GlobalRecordID, formInfoCollectionUri);
            }
            return childResponseList;
        }

        private List<DocumentResponseProperties> ReadAllNewInProcessResponsesByExpression(string expression, Uri formInfoCollectionUri, string collectionAlias = "c")
        {
            var documentResponsePropertiesList = new List<DocumentResponseProperties>();
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
            try
            {
                var query = Client.CreateDocumentQuery(formInfoCollectionUri,
                    SELECT + AssembleSelect(collectionAlias, "GlobalRecordID")
                    + FROM + collectionAlias
                    + WHERE + AssembleWhere(collectionAlias, expression, And_Expression("RecStatus", EQ, RecordStatus.InProcess))
                    , queryOptions);

                documentResponsePropertiesList = query.AsEnumerable()
                    .Select(fi => new DocumentResponseProperties { FormResponseProperties = (FormResponseProperties)fi })
                    .ToList();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return documentResponsePropertiesList;
        }

        #endregion Get hierarchial New InProcess responses by ResponseId

        private class ActionContext
        {
            public ActionContext()
            {
                DeferredActionList = new List<FormResponseProperties>();
            }
            public int? WhereStatus { get; set; }
            public int? NewStatus { get; set; }
            public List<FormResponseProperties> DeferredActionList { get; set; }
        }

        private void StateChangeAction(FormResponseProperties formResponseProperties, ActionContext actionContext)
        {
            if (actionContext.WhereStatus.HasValue && actionContext.WhereStatus.Value != formResponseProperties.RecStatus)
            {
                return;
            }
            if (actionContext.NewStatus.HasValue)
            {
                if (actionContext.NewStatus.Value == RecordStatus.PhysicalDelete)
                {
                    actionContext.DeferredActionList.Add(formResponseProperties);
                }
                else
                {
                    formResponseProperties.RecStatus = actionContext.NewStatus.Value;
                    var result = UpsertFormResponseProperties(formResponseProperties).Result;
                }
            }
        }

        private void PhysicalDeleteDeferredAction(ActionContext actionContext)
        {
            if (actionContext.DeferredActionList.Count > 0)
            {
                actionContext.DeferredActionList.Reverse();
                foreach (var formReponseProperties in actionContext.DeferredActionList)
                {
                    var formName = formReponseProperties.FormName;
                    foreach (var pageId in formReponseProperties.PageIds)
                    {
                    var responseId = formReponseProperties.GlobalRecordID;
                    var pageResponseProperties = GetPageResponsePropertiesByResponseId(responseId, formName, pageId);
                        DeleteSurveyDataInDocumentDB(

                }
            }
        }
    }
}
