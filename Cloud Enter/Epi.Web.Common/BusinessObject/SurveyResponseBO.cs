using System;
using System.Collections.Generic;
using Epi.Cloud.Common.EntityObjects;

namespace Epi.Web.Enter.Common.BusinessObject
{
    public class SurveyResponseBO : ICloneable
    {

        public SurveyResponseBO()
        {
            this.DateUpdated = DateTime.Now;
            this.Status = 1;
        }

        public string ResponseId { get; set; }
        public Guid UserPublishKey { get; set; }
        public string SurveyId { get; set; }
        public DateTime DateUpdated { get; set; }
        public DateTime? DateCompleted { get; set; }
        public int Status { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsDraftMode { get; set; }
        public bool IsLocked { get; set; }
        public string ParentRecordId { get; set; }
        public int UserId { get; set; }
        public string UserEmail { get; set; }
        public string ParentId { get; set; }
        public string RelateParentId { get; set; }
        public bool IsNewRecord { get; set; }
        public List<SurveyResponseBO> ResponseHierarchyIds { get; set; }
        public int ViewId { get; set; }
        public int LastActiveUserId { get; set; }
        public Dictionary<string, string> SqlData { get; set; }
        public int RecordSourceId { get; set; }
        public int CurrentOrgId { get; set; }
        public long? TemplateXMLSize { get; set; }

        string _xml;

        public string XML
        {
            get { return _xml; }
            set { _xml = value; }
        }

        FormResponseDetail _responseDetail;
        public FormResponseDetail ResponseDetail
        {
            get { return _responseDetail; }
            set { _responseDetail = value; }
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
