using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DocumentDBTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void ChildFormResponsePropertiesTest()
        {
            var childFormResponse_f3r1 = new FormResponseProperties_v2
            {
                FormId = "f3",
                ResponseId = "f3.r1",
                ChildFormResponseProperties = new Dictionary<string, FormResponseProperties_v2>()
            };

            var childFormResponse_f2r1 = new FormResponseProperties_v2
            {
                FormId = "f2",
                ResponseId = "f2.r1",
                ChildFormResponseProperties = new Dictionary<string, FormResponseProperties_v2>()
            };

            var childFormResponse_f2r2 = new FormResponseProperties_v2
            {
                FormId = "f2",
                ResponseId = "f2.r2",
                ChildFormResponseProperties = new Dictionary<string, FormResponseProperties_v2>()
            };

            var childFormResponse_f2r3 = new FormResponseProperties_v2
            {
                FormId = "f2",
                ResponseId = "f2.r3",
                //ChildFormResponseProperties = new FormResponseProperties_v2[]
                //{
                //    childFormResponse_f3r1,
                //}.ToDictionary(r => r.ResponseId, r => r)
            };

            var childFormResponse_f1r1 = new FormResponseProperties_v2
            {
                RelateParentResponseId = "f0.r1",
                FormId = "f1",
                ResponseId = "f1.r1",
                //ChildFormResponseProperties = new FormResponseProperties_v2[]
                //{
                //    childFormResponseProperties_f2r1,
                //    childFormResponseProperties_f2r2,
                //    childFormResponseProperties_f2r3
                //}.ToDictionary(r => r.ResponseId, r => r)
            };

            var formResponseResource = new FormResponseResource();
            var rootFormResponse = new FormResponseProperties_v2
            {
                FormId = "root",
                ResponseId = "f0.r1",
                RelateParentResponseId = null
            };
            formResponseResource.FormResponseProperties = rootFormResponse;

            rootFormResponse.AddChildResponse(response: childFormResponse_f1r1);

            childFormResponse_f1r1.AddChildResponse(formResponseResource, childFormResponse_f2r1);
            childFormResponse_f1r1.AddChildResponse(formResponseResource, childFormResponse_f2r2);
            childFormResponse_f1r1.AddChildResponse(formResponseResource, childFormResponse_f2r3);
        }
    }

    //---------------------------------------------------------------------------------------------------

    public class Resource
    {
        public string Id { get; set; }
    }


    public class FormResponseResource : Resource
    {
        public FormResponseResource()
        {
            ChildResponseIdPath = new Dictionary<string, List<string>>();
        }

        public FormResponseProperties_v2 FormResponseProperties { get; set; }
        public Dictionary<string, List<string>> ChildResponseIdPath { get; set; }

        /// <summary>
        /// AddChildFormResponse
        /// </summary>
        /// <param name="childFormResponseProperties"></param>
        /// <returns></returns>
        public FormResponseProperties_v2 AddChildFormResponse(FormResponseProperties_v2 childFormResponseProperties)
        {
            var parentResponseId = childFormResponseProperties.RelateParentResponseId;
            var parentResponse = FindChildFormResponse(parentResponseId);
            parentResponse.AddChildResponse(this, childFormResponseProperties);
            return childFormResponseProperties;
        }

        /// <summary>
        /// FindChildFormResponse
        /// </summary>
        /// <param name="childResponseId"></param>
        /// <returns></returns>
        public FormResponseProperties_v2 FindChildFormResponse(string childResponseId)
        {
            FormResponseProperties_v2 childFormResponseProperties = this.FormResponseProperties;
            List<FormResponseProperties_v2> formResponsePropertiesList = new List<FormResponseProperties_v2>();
            List<string> childResponseIdPath = null;
            string rootResponseId = null;
            if (ChildResponseIdPath.TryGetValue(childResponseId, out childResponseIdPath))
            {
                int pathLength = childResponseIdPath.Count;
                rootResponseId = childResponseIdPath[0];
                for (int pathIndex = 1; pathIndex < pathLength; ++pathIndex)
                {
                    var responseId = childResponseIdPath[pathIndex];
                    childFormResponseProperties = childFormResponseProperties.ChildFormResponseProperties[responseId];
                    if (childResponseId == responseId) break;
                }
            }
            return childFormResponseProperties;
        }

        /// <summary>
        /// AddChildResponseIdPath
        /// </summary>
        /// <param name="childFormResponseProperties"></param>
        public void AddChildResponseIdPath(FormResponseProperties_v2 childFormResponseProperties)
        {
            var responseId = childFormResponseProperties.ResponseId;

            List<string> childResponseIdPath = new List<string>();
            childResponseIdPath.Insert(0, childFormResponseProperties.ResponseId);
            while (childFormResponseProperties.RelateParentResponseId != null)
            {
                var parentResponseId = childFormResponseProperties.RelateParentResponseId;
                childResponseIdPath.Insert(0, parentResponseId);
                childFormResponseProperties = FindChildFormResponse(parentResponseId);
            }
            ChildResponseIdPath[responseId] = childResponseIdPath;
        }
    }
    public partial class FormResponseProperties_v2
    {
        public FormResponseProperties_v2()
        {
            IsNewRecord = true;
            RecStatus = 0;
            ResponseQA = new Dictionary<string, string>();
            ChildFormResponseProperties = new Dictionary<string, FormResponseProperties_v2>();
        }
        public string ResponseId { get; set; }
        public string RootResponseId { get; set; }
        public string RelateParentResponseId { get; set; }
        public string FormId { get; set; }
        public string FormName { get; set; }
        public bool IsNewRecord { get; set; }
        public int RecStatus { get; set; }
        public string UserName { get; set; }
        public string FirstSaveLogonName { get; set; }
        public string LastSaveLogonName { get; set; }
        public DateTime FirstSaveTime { get; set; }
        public DateTime LastSaveTime { get; set; }
        public int UserId { get; set; }
        public bool IsDraftMode { get; set; }
        public string RequiredFieldsList { get; set; }
        public string HiddenFieldsList { get; set; }
        public string HighlightedFieldsList { get; set; }
        public string DisabledFieldsList { get; set; }

        public Dictionary<string, string> ResponseQA { get; set; }

        public Dictionary<string, FormResponseProperties_v2> ChildFormResponseProperties { get; set; }

        public bool IsRootForm { get { return RelateParentResponseId == null; } }
        public bool IsRelatedView { get { return RelateParentResponseId != null; } }

        public void AddChildResponse(FormResponseResource formResponseResource, FormResponseProperties_v2 response)
        {
            response.RootResponseId = formResponseResource.Id;
            response.RelateParentResponseId = ResponseId;
            ChildFormResponseProperties[response.ResponseId] = response;
            formResponseResource.AddChildResponseIdPath(response);
        }
    }
}

