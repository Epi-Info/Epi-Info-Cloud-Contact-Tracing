using System;
using System.Collections.Generic;
using System.Linq;
using Epi.Common.Core.Interfaces;
using Epi.DataPersistence.Constants;
using Epi.DataPersistence.Extensions;

namespace Epi.PersistenceServices.CosmosDB
{
    public partial class FormResponseResource
    {
        public FormResponseProperties AddOrReplaceChildResponse(FormResponseProperties childResponse)
        {
            var parentResponseId = childResponse.ParentResponseId;
            var childFormName = childResponse.FormName;
            var childResponseId = childResponse.ResponseId;

            var childResponseList = GetChildResponseList(parentResponseId, childFormName, /*addIfNoList=*/true);
            var existingResponse = childResponseList.SingleOrDefault(r => r.ResponseId == childResponseId);

            if (existingResponse != null)
            {
                childResponse.CopyTo(existingResponse);
            }
            else
            {
                childResponseList.Add(childResponse);
            }

            // Add the response to the response index if it doesn't alread exist.
            ChildResponseContext existingChildResponseContext = null;
            if (!ChildResponseContexts.TryGetValue(childResponse.ResponseId, out existingChildResponseContext))
            {
                ChildResponseContexts.Add(childResponse.ResponseId, new ChildResponseContext(childResponse));
            }

            var childResponsesByChildFormId = ChildResponses[parentResponseId];
            childResponsesByChildFormId[childFormName] = childResponseList;

            return childResponse;
        }

        public List<FormResponseProperties> GetChildResponseList(IResponseContext responseContext, bool addIfNoList = false, bool includeDeletedRecords = false)
        {
            return GetChildResponseList(responseContext.ParentResponseId, responseContext.FormName, addIfNoList, includeDeletedRecords);
        }

        public List<FormResponseProperties> GetChildResponseList(string parentResponseId, string childFormName, bool addIfNoList = false, bool includeDeletedRecords = false)
        {
            Dictionary<string/*ChildFormId*/, List<FormResponseProperties>> childResponsesByChildFormId = null;
            childResponsesByChildFormId = (ChildResponses.TryGetValue(parentResponseId, out childResponsesByChildFormId)) ? childResponsesByChildFormId : null;
            if (childResponsesByChildFormId == null && addIfNoList)
            {
                ChildResponses.Add(parentResponseId, childResponsesByChildFormId = new Dictionary<string/*ChildFormId*/, List<FormResponseProperties>>());
            }

            List<FormResponseProperties> childResponseList = null;
            if (childResponsesByChildFormId != null)
            {
                childResponseList = (childResponsesByChildFormId.TryGetValue(childFormName, out childResponseList)) ? childResponseList : null;
                if (childResponseList == null && addIfNoList)
                {
                    childResponsesByChildFormId.Add(childFormName, childResponseList = new List<FormResponseProperties>());
                }
            }
            if (childResponseList != null && childResponseList.Count > 0 && includeDeletedRecords == false)
            {
                childResponseList = childResponseList.Where(r => r.RecStatus != RecordStatus.Deleted).ToList();
            }
            return childResponseList;
        }

        public FormResponseProperties GetChildResponse(IResponseContext responseContext)
        {
            return GetChildResponse(responseContext.ParentResponseId, responseContext.FormName, responseContext.ResponseId);
        }

        public FormResponseProperties GetChildResponse(string parentResponseId, string childFormName, string childResponseId)
        {
            var childResponseList = GetChildResponseList(parentResponseId, childFormName);
            var childResponse = childResponseList != null ? childResponseList.SingleOrDefault(r => r.ResponseId == childResponseId) : null;
            return childResponse;
        }

        public void LogicalCascadeDeleteChildren(FormResponseProperties formResponseProperties)
        {
            if (formResponseProperties.RecStatus != RecordStatus.Deleted)
            {
                formResponseProperties.RecStatus = RecordStatus.Deleted;
                CascadeThroughChildren(formResponseProperties, frp => frp.RecStatus = RecordStatus.Deleted);
            }
        }

        public void CascadeThroughChildren(FormResponseProperties formResponseProperties, Action<FormResponseProperties> action)
        {
            Dictionary<string/*ChildFormName*/, List<FormResponseProperties>> childFormResponses = null;
            if (ChildResponses.TryGetValue(formResponseProperties.ResponseId, out childFormResponses))
            {
                // interate over list child forms
                foreach (var childFormResponseList in childFormResponses.Values)
                {
                    // iterate over list of child responses
                    foreach (var childFormResponseProperties in childFormResponseList)
                    {
                        action(childFormResponseProperties);
                        CascadeThroughChildren(childFormResponseProperties, action);
                    }
                }
            }
        }
    }
}
