using Epi.Cloud.Common.Criteria;

namespace Epi.Cloud.Common.Message
{
    /// <summary>
    /// Represents a SurveyInfo request message from client.
    /// </summary>
    public class FormsInfoRequest
    {
        public FormsInfoRequest()
        {
            this.Criteria = new FormInfoCriteria();

        }

        /// <summary>
        /// Selection criteria and sort order
        /// </summary>
        public FormInfoCriteria Criteria;
    }
}
