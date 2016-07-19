using System;

namespace Epi.Cloud.Common.EntityObjects
{
    public class SurveyResponse
    {
        public SurveyResponse()
        {
            ResponseDetail = new FormResponseDetail();
        }

        // DocumentDB Record Id
        public Guid Id { get; set; }

        // DocumentDB timestamp of the most recent update
        public Int64 Timestamp { get; set; }

        public Guid ResponseId { get; set; }
        public Guid SurveyId { get; set; }
        public int PageId { get; set; }
        public Guid ParentRecordId { get; set; }
        public Guid RelateParentId { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public DateTime DateCompleted { get; set; }
        public Int32 StatusId { get; set; }
        public String ResponsePasscode { get; set; }
        public Boolean IsDraftMode { get; set; }
        public Boolean IsLocked { get; set; }
        public Int32 RecordSourceId { get; set; }
        public Int32 OrganizationId { get; set; }
        public Int32 UserId { get; set; }
        public FormResponseDetail ResponseDetail { get; set; }
    }
}
