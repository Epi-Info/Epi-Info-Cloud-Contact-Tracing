using Epi.Cloud.Common.DTO;
using Epi.Cloud.Common.MessageBase;

namespace Epi.Cloud.Common.Message
{
    public class FormsHierarchyRequest : RequestBase
    {
        public FormsHierarchyRequest()
        {
            this.SurveyInfo = new FormInfoDTO();
            this.SurveyResponseInfo = new FormResponseInfoDTO();
        }

        /// <summary>
        /// SurveyInfo object.
        /// </summary>
        public FormInfoDTO SurveyInfo;

        public FormResponseInfoDTO SurveyResponseInfo;
    }
}
