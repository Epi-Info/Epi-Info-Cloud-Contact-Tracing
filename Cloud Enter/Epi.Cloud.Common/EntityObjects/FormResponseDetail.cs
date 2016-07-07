using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Epi.Cloud.Common.EntityObjects
{
    [DataContract]
    public class FormResponseDetail
    {
        public FormResponseDetail()
        {
            HiddenFieldsList = new List<string>();
            HighlightedFieldsList = new List<string>();
            DisabledFieldsList = new List<string>();
            RequiredFieldsList = new List<string>();
            PageResponseDetailList = new List<PageResponseDetail>();
        }

        /// <summary>
        /// FormId (aka SurveyId)
        /// </summary>
        [DataMember]
        public string FormId { get; set; }
        [DataMember]
        public int LastPageVisited { get; set; }
        [DataMember]
        public List<string> HiddenFieldsList { get; set; }
        [DataMember]
        public List<string> HighlightedFieldsList { get; set; }
        [DataMember]
        public List<string> DisabledFieldsList { get; set; }
        [DataMember]
        public List<string> RequiredFieldsList { get; set; }
        [DataMember]
        public List<PageResponseDetail> PageResponseDetailList{ get; set; }

        public Dictionary<string, string> FlattenedResponseQA(Func<string, string> keyModifier = null) 
        {
            var flattenedResponseQA = new Dictionary<string, string>();
            foreach (var pageResponseDetail in PageResponseDetailList)
            {
                foreach (var qa in pageResponseDetail.ResponseQA)
                    flattenedResponseQA[keyModifier != null ? keyModifier(qa.Key) : qa.Key] = qa.Value;
            }
            return flattenedResponseQA;
        }
    }

    [DataContract]
    public class PageResponseDetail
    {
        public PageResponseDetail()
        {
            ResponseQA = new Dictionary<string, string>();
        }

        [DataMember]
        public int PageNumber { get; set; }
        [DataMember]
        public int PageId { get; set; }
        [DataMember]
        public int MetadataPageId { get; set; }
        [DataMember]
        public Dictionary<string, string> ResponseQA { get; set; }
    }
}
