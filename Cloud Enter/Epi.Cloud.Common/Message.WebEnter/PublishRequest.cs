using Epi.Cloud.Common.DTO;

namespace Epi.Cloud.Common.Message
{
    public class PublishRequest //: Epi.Cloud.Common.MessageBase.RequestBase
    {

        public PublishRequest()
        {
            this.SurveyInfo = new SurveyInfoDTO();
        }

        /// <summary>
        /// SurveyInfo object.
        /// </summary>
        public SurveyInfoDTO SurveyInfo;
    }
}
