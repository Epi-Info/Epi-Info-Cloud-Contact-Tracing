using System;
using Epi.Common.Core.DataStructures;
using Epi.Common.Core.Interfaces;

namespace Epi.Cloud.Common.DTO
{
    public class SurveyAnswerStateDTO
    {
        public ResponseContext ResponseContext { get; set; }

        // Fields that are used in CheckForConcurrency
        public string ResponseId { get; set; }
        public int LoggedInUserId { get; set; }

        public int LastActiveUserId { get; set; }

        public int Status { get; set; }

        public DateTime DateUpdated { get; set; }
    }
}
