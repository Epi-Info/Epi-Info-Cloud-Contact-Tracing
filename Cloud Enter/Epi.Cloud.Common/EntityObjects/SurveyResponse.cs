using System;
using System.Collections.Generic;

namespace Epi.Cloud.Common.EntityObjects
{
    public class SurveyResponse
    {
        public Guid ResponseId { get; set; }
        public Guid SurveyId { get; set; }
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
