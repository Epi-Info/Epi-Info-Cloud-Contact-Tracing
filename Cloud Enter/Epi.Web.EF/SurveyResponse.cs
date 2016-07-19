using System;
using Epi.Cloud.Common.EntityObjects;

namespace Epi.Web.EF
{
    public partial class SurveyResponse
    {
        public Int32 UserId { get; set; }
        public FormResponseDetail ResponseDetail { get; set; }
    }
}
